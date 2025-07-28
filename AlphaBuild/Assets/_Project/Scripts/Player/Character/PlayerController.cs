using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

[Flags]
public enum EPlayerState
{
    Idle = 1 << 0,
    Moving = 1 << 1,
    Falling = 1 << 2,
    Dashing = 1 << 3,
    Shooting = 1 << 4,
    Zooming = 1 << 5,

    RotateState = Moving | Shooting | Zooming,
}

public class PlayerController : MonoBehaviour, PlayerActions.IPlayerActionMapActions
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterStat stats;
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject followCameraPrefab;
    private FollowCameraController _followCamera;

    [Header("State")]
    [SerializeField] private EPlayerState movementBlockMask = EPlayerState.Dashing;
    [SerializeField] private EPlayerState dashBlockMask;
    [SerializeField] private EPlayerState shootBlockMask = EPlayerState.Dashing;
    [SerializeField] private EPlayerState zoomBlockMask = EPlayerState.Dashing;
    private EPlayerState _currentPlayerState = EPlayerState.Idle;

    [Header("Movement")]
    [SerializeField, Range(0.01f, 100.0f)] private float rotationSpeed = 40.0f;
    private PlayerActions _playerActions;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _totalDirection = Vector3.zero;
    private bool _canMove = true;

    [Header("Gravity")]
    [SerializeField] private Vector3 boxSize = new Vector3(0.2f, 0.01f, 0.2f);
    [SerializeField] private float gravityScale = 2.0f;
    private bool _isGrounded = false;
    private Vector3 _fallVelocity;

    [Header("Dash")]
    private Vector3 _dashDirection = Vector3.zero;
    private float _dashSpeed = 0.0f;
    #endregion

    #region Properties
    public CharacterStat Stats
    {
        get => stats;
    }

    public Vector3 FallVelocity
    {
        get { return _fallVelocity; }
    }

    public Vector3 DashDirection
    {
        get { return _dashDirection; }
        set { _dashDirection = value; }
    }

    public float DashSpeed
    {
        get { return _dashSpeed; }
        set { _dashSpeed = value; }
    }

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float bulletSpeed = 20.0f;
    [SerializeField] private float shootCooldown = 0.5f;
    [SerializeField] private float bulletScale = 0.05f;
    private bool _isShooting = false;
    private bool _isRotating = false;
    private float _lastShootTime = -Mathf.Infinity;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _playerActions = new PlayerActions();
        _playerActions.PlayerActionMap.SetCallbacks(this);

        _followCamera = FindFirstObjectByType<FollowCameraController>();
        if (_followCamera == null)
        {
            GameObject cameraObject = Instantiate(followCameraPrefab);
            cameraObject.name = followCameraPrefab.name;
            _followCamera = cameraObject.GetComponent<FollowCameraController>();
        }
        _followCamera.InitFollowCamera(this);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        HandleMove();
        DashMove();

        // Shoot();

        HandleGravity();

        _followCamera.UpdateFollowCamera();
        RotateCharacter();

        characterController.Move(_totalDirection * Time.deltaTime);
        _totalDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        _playerActions.PlayerActionMap.Disable();
    }
    #endregion

    #region Input Actions
    void PlayerActions.IPlayerActionMapActions.OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
        if ((_currentPlayerState & movementBlockMask) != 0) return;
    }

    void PlayerActions.IPlayerActionMapActions.OnDash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if ((_currentPlayerState & dashBlockMask) != 0) return;

            if (_moveInput == null || _moveInput == Vector2.zero)
            {
                Vector3 camForward = _followCamera.transform.forward;
                camForward.y = 0.0f;
                _dashDirection = camForward.normalized;
            }
            else
            {
                _dashDirection = CalculateInputDirection();
            }

            PartBase ability = inventory.EquippedItems[EPartType.Legs].GetComponent<PartBase>();
            if (ability != null)
            {
                ability.UseAbility();
            }
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnZoom(InputAction.CallbackContext context)
    {
        if ((_currentPlayerState & zoomBlockMask) != 0) return;

        if (context.started)
        {
            _currentPlayerState &= ~EPlayerState.Shooting;
            _currentPlayerState |= EPlayerState.Zooming;
            _followCamera.IsBeforeZoom = true;
            _followCamera.IsZoomed = true;
            animator.SetBool("isAim", true);
        }

        if (context.canceled)
        {
            _currentPlayerState &= ~EPlayerState.Shooting;
            _currentPlayerState &= ~EPlayerState.Zooming;
            _followCamera.IsBeforeZoom = false;
            _followCamera.IsZoomed = false;
            animator.SetBool("isAim", false);
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnLeftAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if ((_currentPlayerState & shootBlockMask) != 0) return;

            _currentPlayerState |= EPlayerState.Shooting;
            _isShooting = true;
        }
        
        if (context.canceled)
        {
            _currentPlayerState &= ~EPlayerState.Shooting;
            _isShooting = false;
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnRightAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Right Attack Triggered");
    }
    #endregion

    #region Public Methods
    public void Dash(float dashSpeed)
    {
        _dashSpeed = dashSpeed;

        _currentPlayerState &= ~EPlayerState.Idle;
        _currentPlayerState &= ~EPlayerState.Moving;
        _currentPlayerState |= EPlayerState.Dashing;
    }

    public void FinishDash()
    {
        _dashDirection = Vector3.zero;
        _dashSpeed = 0.0f;

        _currentPlayerState &= ~EPlayerState.Dashing;
        SwitchStateToIdle();
    }

    public void PartJump(float jumpVelocity)
    {
        if (!_isGrounded) return;

        Vector3 forward = _followCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();

        _fallVelocity = forward * jumpVelocity * 0.5f;
        _fallVelocity.y = jumpVelocity;

        _fallVelocity.y = jumpVelocity;
        _totalDirection += _fallVelocity;
    }

    // Need to move values to parts.
    public void PartShoot(float speed, float size, float cooldown, float xValue, float yValue, Vector3 shakeValue, float damage)
    {
        bulletSpeed = speed;
        bulletScale = size;
        shootCooldown = cooldown;
        _followCamera.SetRecoilValue(xValue, yValue, shakeValue);
    }

    public void SetPartStat(PartBase part)
    {
        stats.SetPartStats(part.PartType, part.Stats);
    }

    public void TakeDamage(int takeDamage)
    {
        Debug.Log($"Player에게 {takeDamage} 데미지! 효과는 굉장했다!");
    }

    public void SetMovable(bool canMove)
    {
        _canMove = canMove;
        animator.enabled = canMove;
    }
    #endregion

    #region Private Methods
    private void HandleMove()
    {
        if (!_canMove) return;
        if ((_currentPlayerState & movementBlockMask) != 0) return;

        if (_moveInput == null || _moveInput == Vector2.zero)
        {
            SwitchStateToIdle();
            return;
        }

        _currentPlayerState &= ~EPlayerState.Idle;
        _currentPlayerState |= EPlayerState.Moving;
        _moveDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;
        animator.SetFloat("moveX", _moveDirection.x);
        animator.SetFloat("moveY", _moveDirection.z);
        animator.SetFloat("moveMagnitude", _moveDirection.magnitude);

        _totalDirection += CalculateInputDirection() * stats.TotalStats[EStatType.MoveSpeed].Value;
    }

    private void SwitchStateToIdle()
    {
        _currentPlayerState &= ~EPlayerState.Moving;
        _currentPlayerState |= EPlayerState.Idle;
        _moveDirection = Vector3.zero;
        animator.SetFloat("moveX", 0.0f);
        animator.SetFloat("moveY", 0.0f);
        animator.SetFloat("moveMagnitude", 0.0f);
    }

    private Vector3 CalculateInputDirection()
    {
        Vector3 camForward = _followCamera.transform.forward;
        Vector3 camRight = _followCamera.transform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 camDirection = camForward * _moveInput.y + camRight * _moveInput.x;

        return camDirection.normalized;
    }

    private void DashMove()
    {
        if (!_canMove) return;
        if (_currentPlayerState != EPlayerState.Dashing) return;

        _totalDirection += _dashDirection * _dashSpeed;
        _followCamera.CameraTarget.position = transform.position + new Vector3(0.0f, 1.2f, 0.0f);
    }

    private void HandleGravity()
    {
        _isGrounded = Physics.CheckBox(groundCheck.position, boxSize, Quaternion.identity);
        if (_isGrounded && _fallVelocity.y <= 0.0f)
        {
            _currentPlayerState &= ~EPlayerState.Falling;
            _fallVelocity = Vector3.zero;
            return;
        }
        _currentPlayerState |= EPlayerState.Falling;
        _fallVelocity.y += -9.8f * gravityScale * Time.deltaTime;
        _totalDirection += _fallVelocity;
    }

    private void RotateCharacter()
    {
        if ((_currentPlayerState & EPlayerState.RotateState) == 0) return;

        Vector3 lookDirection = _followCamera.transform.forward;

        //Camera cam = Camera.main;
        //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //RaycastHit hit;
        //Vector3 targetPoint;

        //if (Physics.Raycast(ray, out hit, 100.0f))
        //{
        //    targetPoint = hit.point;
        //}
        //else
        //{
        //    targetPoint = ray.origin + ray.direction * 100.0f;
        //}
        //lookDirection = (targetPoint - _followCamera.CameraTarget.transform.position).normalized;

        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            transform.forward = Vector3.Slerp(transform.forward, lookDirection, rotationSpeed * Time.deltaTime);
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

        Inventory inven = GetComponent<Inventory>();
        PartBase ability = inven.EquippedItems[EPartType.Shoulder].GetComponent<PartBase>();
        if (ability != null)
        {
            ability.UseAbility();
        }

        _followCamera.ApplyRecoil();

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.transform.localScale = Vector3.one * bulletScale;
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.from = gameObject;
            bulletComponent.damage = (int)1;
        }
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = camShootDirection * bulletSpeed;
        }
        Destroy(bullet, 3.0f);
        _lastShootTime = Time.time;
    }
    #endregion
}
