using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int hp = 100;
    [SerializeField] private float speed = 7.0f;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject _weapon;
    [SerializeField] private Transform _weaponSocketPos;

    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float debugLineLength = 10.0f;
    [SerializeField] private float shootCooltime = 0.1f;
    private float curShootCooltime = 0.0f;
    Vector3 targetPosition = Vector3.zero;

    [Header("Camera Settings")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _zoomCamera;
    [SerializeField] private float _transitionSpeed = 5.0f;
    [SerializeField] private GameObject _aimUI;
    private Vector3 _cameraVelocity = Vector3.zero;
    private bool _isAiming = false;

    private CharacterController _characterController;
    private Vector3 _moveDirection;
    private Vector3 _gravityVelocity;
    private bool _isGrounded;

    private Animator _animator;
    
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject newWeapon = Instantiate(_weapon);
        newWeapon.transform.SetParent(_weaponSocketPos.transform, false);
        newWeapon.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastForBulletPos();

        HandleRotation();
        HandleMovement();
        HandleGravity();

        InputMouseButtonLeft();
        InputMouseButtonRight();

        //var mouse = Input.GetAxis("Mouse X");
        //transform.Rotate(new Vector3(0.0f, mouse * sensitivity, 0.0f));

        curShootCooltime += Time.deltaTime;
    }

    private void HandleGravity()
    {
        // _moveDirection.y += gravity * Time.deltaTime;
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (_isGrounded && _gravityVelocity.y < 0)
        {
            _gravityVelocity.y = -2f; // Reset to a small negative value to keep grounded
        }

        _gravityVelocity.y += gravity * Time.deltaTime;
        _characterController.Move(_gravityVelocity * Time.deltaTime);
    }

    private void InputMouseButtonLeft()
    {
        if (!Input.GetMouseButton(0)) return;
        if (curShootCooltime < shootCooltime) return;

        if (_isAiming)
        {
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<Bullet>().SetBulletDirection(targetPosition - bulletSpawnPoint.position);
            _animator.SetTrigger("ShootTrigger");
            curShootCooltime = 0.0f;
        }
        else
        {
            // Melee attack
        }
    }

    private void InputMouseButtonRight()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _isAiming = true;
            _animator.SetBool("IsAiming", true);
            _aimUI.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isAiming = false;
            _animator.SetBool("IsAiming", false);
            _aimUI.SetActive(false);
        }

        Transform target = _isAiming ? _zoomCamera.transform : _mainCamera.transform;
        float targetFOV = _isAiming ? _zoomCamera.fieldOfView : _mainCamera.fieldOfView;
        Transform current = Camera.main.transform;

        current.position = Vector3.SmoothDamp(current.position, target.position, ref _cameraVelocity, _transitionSpeed * Time.deltaTime);
        current.rotation = Quaternion.Slerp(current.rotation, target.rotation, _transitionSpeed * Time.deltaTime);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, _transitionSpeed * Time.deltaTime);
    }

    private void HandleMovement()
    {
        var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        if (input == Vector3.zero)
        {
            _animator.SetBool("IsMoving", false);
            return;
        }

        _moveDirection = transform.TransformDirection(input) * speed;
        _characterController.Move(_moveDirection * Time.deltaTime);
        _animator.SetBool("IsMoving", true);
    }

    private void HandleRotation()
    {
        var mouse = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up, mouse * sensitivity);
    }

    private void RaycastForBulletPos()
    {
        Transform cameraTransform = Camera.main.transform;
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position + (cameraTransform.forward), cameraTransform.forward, out hit, debugLineLength, mask))
        {
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = cameraTransform.position + cameraTransform.forward * debugLineLength;
        }
    }

    #region MyRegion

    public void TakeDamage(int damage)
    {
        Debug.Log($"TAKE DAMAGE {damage}");
        hp -= damage;
    }

    #endregion
}
