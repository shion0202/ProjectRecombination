using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

public class PlayerController : MonoBehaviour, PlayerActions.IPlayerActionMapActions
{
    #region Variables
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float zoomedMoveSpeed = 2.0f;
    private CharacterController _characterController;
    private PlayerActions _playerActions;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _totalDirection = Vector3.zero;
    private Dictionary<ECameraState, float> _moveSpeeds = new Dictionary<ECameraState, float>();

    [Header("Dash")]
    [SerializeField, Tooltip("대시에 필요한 수치 (X: 대시 거리, Y: 대시 유지 시간, Z: 대시 쿨타임)")]
    private Vector3 dashValues = new Vector3(5.0f, 0.1f, 5.0f);
    private bool _isDashing = false;
    private bool _canDash = true;
    private Vector3 _dashDirection = Vector3.zero;
    private Coroutine _dashCoroutine = null;

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 20.0f;
    [SerializeField] private float shootCooldown = 0.5f;
    [SerializeField] private float bulletSpreadRange = 0.2f;
    private bool _isShooting = false;
    private bool _isRotating = false;
    private float _lastShootTime = -Mathf.Infinity;

    [Header("Gravity")]
    [SerializeField] private Vector3 boxSize = new Vector3(0.2f, 0.01f, 0.2f);
    [SerializeField] private float gravityScale = 2.0f;
    private Transform _groundCheck;
    private bool _isGrounded = false;
    private Vector3 _fallVelocity;

    public Vector3 FallVelocity
    {
        get { return _fallVelocity; }
    }

    [Header("Camera")]
    [SerializeField] private GameObject followCameraPrefab;
    [SerializeField] private float rotationSpeedByCamera = 40.0f;
    private FollowCameraController _followCamera;

    [Header("Animation")]
    private Animator _animator;
    #endregion

    #region Unity Events
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _groundCheck = GetComponentInChildren<GroundCheck>().transform;
        _animator = GetComponentInChildren<Animator>();

        _moveSpeeds.Add(ECameraState.Normal, moveSpeed);
        _moveSpeeds.Add(ECameraState.Zoom, zoomedMoveSpeed);

        _playerActions = new PlayerActions();
        _playerActions.PlayerActionMap.SetCallbacks(this);

        _followCamera = FindFirstObjectByType<FollowCameraController>();
        if (_followCamera == null)
        {
            GameObject cameraObject = Instantiate(followCameraPrefab);
            cameraObject.name = followCameraPrefab.name;
            _followCamera = cameraObject.GetComponent<FollowCameraController>();
        }
        _followCamera.SetFollowCamera(this);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        _playerActions.PlayerActionMap.Enable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Inventory inven = GetComponent<Inventory>();
            IPartAbility ability = inven.EquippedItems[EEquipmentType.Legs].GetComponent<IPartAbility>();
            if (ability != null)
            {
                ability.UseAbility(this);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        HandleMove();
        DashMove();

        Shoot();

        HandleGravity();

        _followCamera.UpdateFollowCamera();
        RotateCharacter();

        _characterController.Move(_totalDirection * Time.deltaTime);
        _totalDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        _playerActions.PlayerActionMap.Disable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isGrounded ? Color.red : Color.green;
        // Gizmos.DrawWireCube(_groundCheck.position, boxSize);
    }
    #endregion

    #region Input Actions
    void PlayerActions.IPlayerActionMapActions.OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _moveInput = Vector2.zero;
            _moveDirection = Vector3.zero;
            _animator.SetFloat("moveX", 0.0f);
            _animator.SetFloat("moveY", 0.0f);
            _animator.SetFloat("moveMagnitude", 0.0f);
            return;
        }

        _moveInput = context.ReadValue<Vector2>();
        if (_moveInput != null && _moveInput != Vector2.zero)
        {
            _moveDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

            _animator.SetFloat("moveX", _moveDirection.x);
            _animator.SetFloat("moveY", _moveDirection.z);
            _animator.SetFloat("moveMagnitude", _moveDirection.magnitude);
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnDash(InputAction.CallbackContext context)
    {
        if (!_canDash || _followCamera.IsZoomed) return;

        if (context.started)
        {
            // If press the button, dash to pressed direction.
            // Need distance and time(or speed).
            // Can't move, and has cooldown

            if (_dashCoroutine != null)
            {
                StopCoroutine(_dashCoroutine);
                _dashCoroutine = null;
            }
            _dashCoroutine = StartCoroutine(CoHandleDash());
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnZoom(InputAction.CallbackContext context)
    {
        if (_isDashing) return;

        // Devide started and canceled state.
        if (context.started)
        {
            _followCamera.IsZoomed = true;
        }
        else if (context.canceled)
        {
            _followCamera.IsZoomed = false;
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && !_isDashing)
        {
            _isShooting = true;
        }
        else if (context.canceled)
        {
            _isShooting = false;
        }
    }
    #endregion

    #region Functions
    public void TakeDamage(int takeDamage)
    {
        Debug.Log($"{takeDamage}만큼의 피해를 입었습니다.");
    }

    public void PartDash()
    {
        if (!_canDash || _followCamera.IsZoomed) return;

        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }
        _dashCoroutine = StartCoroutine(CoHandleDash());
    }

    public void PartJump(float jumpVelocity)
    {
        if (!_isGrounded) return;

        if (_isGrounded)
        {
            _fallVelocity.y = jumpVelocity;
        }

        _totalDirection += _fallVelocity;
    }

    private void HandleMove()
    {
        if (_moveInput == null || _moveInput == Vector2.zero) return;
        if (_isDashing) return;

        // move to looking direction
        Vector3 camForward = _followCamera.transform.forward;
        Vector3 camRight = _followCamera.transform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 camMoveDirection = camForward * _moveInput.y + camRight * _moveInput.x;

        float curMoveSpeed = _moveSpeeds[_followCamera.CurrentCameraState];
        // _characterController.Move(camMoveDirection * moveSpeed * Time.deltaTime);
        _totalDirection += camMoveDirection * curMoveSpeed;
    }

    private void DashMove()
    {
        if (!_isDashing) return;

        float dashSpeed = dashValues.x / dashValues.y;
        // _characterController.Move(_dashDirection * dashSpeed * Time.deltaTime);
        _totalDirection += _dashDirection * dashSpeed;
        _followCamera.CameraTarget.position = transform.position + new Vector3(0.0f, 1.2f, 0.0f);
    }

    private void HandleGravity()
    {
        // Fall by gravity.
        // Check the floor.
        // Calculate y velocity by gravity.
        _isGrounded = Physics.CheckBox(_groundCheck.position, boxSize, Quaternion.identity);
        if (_isGrounded && _fallVelocity.y < 0.0f)
        {
            _fallVelocity.y = 0.0f;
        }
        else
        {
            _fallVelocity.y += -9.8f * Time.deltaTime * gravityScale;
        }

        // _gravityMovement.y = _velocity.y * Time.deltaTime;
        // _characterController.Move(_gravityMovement);
        _totalDirection += _fallVelocity;
    }

    private void RotateCharacter()
    {
        if (_moveDirection.magnitude <= 0.1f && !_followCamera.IsZoomed) return;

        Vector3 lookDirection = _followCamera.transform.forward;
        if (_followCamera.IsZoomed)
        {
            Camera cam = Camera.main;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * 100.0f;
            }
            lookDirection = (targetPoint - _followCamera.CameraTarget.transform.position).normalized;
        }
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, lookDirection, rotationSpeedByCamera * Time.deltaTime);
        }
    }

    private void Shoot()
    {
        if (!_isShooting || Time.time < _lastShootTime + shootCooldown) return;

        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100.0f;
        }
        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;

        if (!_followCamera.IsZoomed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camShootDirection, Vector3.up);
            targetRotation.x = 0.0f;
            targetRotation.z = 0.0f;
            transform.rotation = targetRotation;
        }

        _followCamera.ApplyRecoil();

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = camShootDirection * bulletSpeed;
        }
        Destroy(bullet, 3.0f);
        _lastShootTime = Time.time;
    }

    IEnumerator CoHandleDash()
    {
        Vector3 _dashStartPos = _followCamera.CameraTarget.position;
        _isDashing = true;
        _canDash = false;

        Vector3 camForward = _followCamera.transform.forward;
        Vector3 camRight = _followCamera.transform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 camDashDirection = camForward * _moveInput.y + camRight * _moveInput.x;

        if (_moveInput == null || _moveInput == Vector2.zero)
        {
            _dashDirection = camForward.normalized;
        }
        else
        {
            _dashDirection = camDashDirection.normalized;
        }

        CinemachineVirtualCamera vcam = _followCamera.transform.GetComponent<CinemachineVirtualCamera>();
        vcam.OnTargetObjectWarped(_followCamera.CameraTarget, _followCamera.CameraTarget.position - _dashStartPos);

        yield return new WaitForSeconds(dashValues.y);

        _isDashing = false;

        yield return new WaitForSeconds(dashValues.z);

        vcam.OnTargetObjectWarped(_followCamera.CameraTarget, _followCamera.CameraTarget.position - _dashStartPos);
        _canDash = true;
    }
    #endregion
}
