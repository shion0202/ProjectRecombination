using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerActions.IPlayerActionMapActions
{
    #region Variables
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float lookRotationSpeed = 720.0f;
    private CharacterController _characterController;
    private PlayerActions _playerActions;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _totalDirection = Vector3.zero;

    [Header("Gravity")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 boxSize;
    private bool _isGrounded = false;
    private Vector3 _gravityMovement;
    private Vector3 _velocity;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 2.0f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;
    private bool _isDashing = false;
    private bool _canDash = true;
    private Vector3 _dashStartPos = Vector3.zero;
    private Vector3 _dashDirection = Vector3.zero;
    private Coroutine _dashCoroutine = null;

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 20.0f;
    [SerializeField] private float shootCooldown = 0.5f;
    [SerializeField] private float bulletSpread = 0.2f;
    private bool _isShooting = false;
    private bool _isRotating = false;
    private float _lastShootTime = -Mathf.Infinity;

    [Header("Transform")]
    [SerializeField] List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
    [SerializeField] List<Material> normalMaterials = new List<Material>();
    [SerializeField] List<Material> transformedMaterials = new List<Material>();
    [SerializeField] private float rushDistance = 5.0f;
    [SerializeField] private float rushDuration = 0.1f;
    [SerializeField] private float rushCooldown = 1.0f;
    private bool _isTransformed = false;
    private bool _isRushing = false;
    private bool _canRush = true;
    private Vector3 _rushStartPos = Vector3.zero;
    private Vector3 _rushDirection = Vector3.zero;
    private Coroutine _rushCoroutine = null;

    [Header("Camera")]
    [SerializeField] private Transform cameraTrasnform;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float zoomFOV = 30.0f;
    [SerializeField] private Vector3 zoomOffset = Vector3.zero;
    [SerializeField] private float normalFOV = 60.0f;
    [SerializeField] private Vector3 normalOffset = Vector3.zero;
    [SerializeField] private float zoomSpeed = 10.0f;
    [SerializeField] private float cameraRotationSpeed = 20.0f;
    private bool _isZoomed = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    #endregion

    #region Unity Events
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _playerActions = new PlayerActions();
        _playerActions.PlayerActionMap.SetCallbacks(this);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        _playerActions.PlayerActionMap.Enable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        HandleGravity();
        HandleZoom();

        HandleMove();
        DashMove();

        if (_isTransformed)
        {
            MeleeAttack();
        }
        else
        {
            Shoot();
        }

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
        Gizmos.DrawWireCube(groundCheck.position, boxSize);
    }
    #endregion

    #region Input Actions
    void PlayerActions.IPlayerActionMapActions.OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            _moveInput = Vector2.zero;
            _moveDirection = Vector3.zero;
            animator.SetFloat("moveX", 0.0f);
            animator.SetFloat("moveY", 0.0f);
            animator.SetFloat("moveMagnitude", 0.0f);
            return;
        }

        _moveInput = context.ReadValue<Vector2>();
        if (_moveInput != null && _moveInput != Vector2.zero)
        {
            _moveDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;

            if (_isZoomed)
            {
                animator.SetFloat("moveX", _moveDirection.x);
                animator.SetFloat("moveY", _moveDirection.z);
                animator.SetFloat("moveMagnitude", _moveDirection.magnitude);
            }
            else
            {
                animator.SetFloat("moveY", 1.0f);
                animator.SetFloat("moveMagnitude", _moveDirection.magnitude);
            }
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && _canDash)
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
        if (context.performed)
        {
            if (_isTransformed) return;

            _isZoomed = !_isZoomed;
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_isTransformed && _canRush)
            {
                if (_rushCoroutine != null)
                {
                    StopCoroutine(_rushCoroutine);
                    _rushCoroutine = null;
                }
                _rushCoroutine = StartCoroutine(CoHandleMeleeAttack());
            }
            else
            {
                _isShooting = true;
            }
        }
        else if (context.canceled)
        {
            _isShooting = false;
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnTransformation(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isTransformed = !_isTransformed;
            _isZoomed = false;
            if (_isTransformed)
                animator.SetBool("isTransformed", true);
            else
                animator.SetBool("isTransformed", false);

            ChangeMaterial();
        }
    }
    #endregion

    #region Functions
    private void HandleMove()
    {
        if (_moveInput == null || _moveInput == Vector2.zero) return;
        if (_isDashing || _isRushing) return;

        // move to looking direction
        Vector3 camForward = cameraTrasnform.forward;
        Vector3 camRight = cameraTrasnform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 camMoveDirection = camForward * _moveInput.y + camRight * _moveInput.x;

        // _characterController.Move(camMoveDirection * moveSpeed * Time.deltaTime);
        _totalDirection += camMoveDirection * moveSpeed;

        if (!_isZoomed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, lookRotationSpeed * Time.deltaTime);
        }
    }

    private void DashMove()
    {
        if (!_isDashing) return;

        float dashSpeed = dashDistance / dashDuration;
        // _characterController.Move(_dashDirection * dashSpeed * Time.deltaTime);
        _totalDirection += _dashDirection * dashSpeed;

        cameraTarget.position = transform.position + new Vector3(0.0f, 1.2f, 0.0f);
    }

    private void HandleGravity()
    {
        // Fall by gravity
        // Check the floor.
        // Calculate y velocity by gravity.
        _isGrounded = Physics.CheckBox(groundCheck.position, boxSize, Quaternion.identity);
        if (_isGrounded && _velocity.y < 0.0f)
        {
            _velocity.y = 0.0f;
        }
        else
        {
            _velocity.y += -9.8f * Time.deltaTime;
        }

        // _gravityMovement.y = _velocity.y * Time.deltaTime;
        // _characterController.Move(_gravityMovement);
        _totalDirection += _velocity;
    }

    private void HandleZoom()
    {
        animator.SetBool("isAim", _isZoomed);

        float targetFOV = _isZoomed ? zoomFOV : normalFOV;
        CinemachineVirtualCamera vcam = cameraTrasnform.GetComponent<CinemachineVirtualCamera>();
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, zoomSpeed * Time.deltaTime);

        // x, y: screenX and screenY
        // z: cameraDistance
        Vector3 targetOffset = _isZoomed ? zoomOffset : normalOffset;
        var framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        framingTransposer.m_CameraDistance = Mathf.Lerp(framingTransposer.m_CameraDistance, targetOffset.z, zoomSpeed * Time.deltaTime);
        framingTransposer.m_ScreenX = Mathf.Lerp(framingTransposer.m_ScreenX, targetOffset.x, zoomSpeed * Time.deltaTime);
        framingTransposer.m_ScreenY = Mathf.Lerp(framingTransposer.m_ScreenY, targetOffset.y, zoomSpeed * Time.deltaTime);

        RotateCharacter();
    }

    private void RotateCharacter()
    {
        if (!_isZoomed) return;

        CinemachineVirtualCamera vcam = cameraTrasnform.GetComponent<CinemachineVirtualCamera>();
        Vector3 lookDirection = vcam.transform.forward;
        lookDirection.y = 0;

        // transform.forward = lookDirection;
        if (lookDirection.sqrMagnitude > 0.01f)
            transform.forward = Vector3.Lerp(transform.forward, lookDirection, cameraRotationSpeed * Time.deltaTime);
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
            if (!_isZoomed)
            {
                Vector3 spreadValue = new Vector3(UnityEngine.Random.Range(bulletSpread * -1.0f, bulletSpread), UnityEngine.Random.Range(bulletSpread * -1.0f, bulletSpread), 0.0f);
                targetPoint += spreadValue;
            }
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100.0f;
        }
        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;

        if (!_isZoomed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camShootDirection, Vector3.up);
            targetRotation.x = 0.0f;
            targetRotation.z = 0.0f;
            transform.rotation = targetRotation;
        }

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = camShootDirection * bulletSpeed;
        }
        Destroy(bullet, 3.0f);
        _lastShootTime = Time.time;
    }

    private void MeleeAttack()
    {
        if (!_isRushing) return;

        // dash to front, and attack
        float rushSpeed = rushDistance / rushDuration;
        _totalDirection += _rushDirection * rushSpeed;
    }

    private void ChangeMaterial()
    {
        for (int i = 0; i < renderers.Count; ++i)
        {
            Renderer renderer = renderers[i];

            if (_isTransformed)
            {
                renderer.material = transformedMaterials[i];
            }
            else
            {
                renderer.material = normalMaterials[i];
            }
        }
    }

    IEnumerator CoHandleDash()
    {
        _dashStartPos = cameraTarget.position;
        _isDashing = true;
        _canDash = false;

        Vector3 camForward = cameraTrasnform.forward;
        Vector3 camRight = cameraTrasnform.right;
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

        CinemachineVirtualCamera vcam = cameraTrasnform.GetComponent<CinemachineVirtualCamera>();
        vcam.OnTargetObjectWarped(cameraTarget, cameraTarget.position - _dashStartPos);

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        vcam.OnTargetObjectWarped(cameraTarget, cameraTarget.position - _dashStartPos);
        _canDash = true;
    }

    IEnumerator CoHandleMeleeAttack()
    {
        _rushStartPos = cameraTarget.position;
        _isRushing = true;
        _canRush = false;

        Vector3 camForward = cameraTrasnform.forward;
        camForward.y = 0.0f;
        camForward.Normalize();
        _rushDirection = camForward;

        Quaternion targetRotation = Quaternion.LookRotation(_rushDirection, Vector3.up);
        targetRotation.x = 0.0f;
        targetRotation.z = 0.0f;
        transform.rotation = targetRotation;

        animator.SetBool("isAttack", true);

        yield return new WaitForSeconds(rushDuration);
        
        _isRushing = false;
        animator.SetBool("isAttack", false);

        yield return new WaitForSeconds(rushCooldown);

        _canRush = true;
    }
    #endregion
}
