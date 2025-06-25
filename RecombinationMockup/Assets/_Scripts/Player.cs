using System.Collections;
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
    [SerializeField] private GameObject _secondWeapon;
    [SerializeField] private Transform _weaponSocketPos;
    private GameObject _curWeapon = null;
    private int _weaponIndex = 0;
    private Coroutine _weaponCoroutine = null;
    private bool _isWeaponChanging = false;

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
    private bool _isZooming = false;

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
        _curWeapon = Instantiate(_weapon);
        _curWeapon.transform.SetParent(_weaponSocketPos.transform, false);
        _curWeapon.transform.localPosition = Vector3.zero;
        _weaponIndex = 0;
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

        ChangeWeapon();

        //var mouse = Input.GetAxis("Mouse X");
        //transform.Rotate(new Vector3(0.0f, mouse * sensitivity, 0.0f));

        curShootCooltime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void OnWeaponChange()
    {
        Destroy(_curWeapon);
        _curWeapon = Instantiate(_weaponIndex == 0 ? _weapon : _secondWeapon);
        _curWeapon.transform.SetParent(_weaponSocketPos.transform, false);
        _curWeapon.transform.localPosition = Vector3.zero;
    }

    private void ChangeWeapon()
    {
        if (_isAiming) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_weaponIndex == 0) return;

            _weaponIndex = _weaponIndex == 0 ? 1 : 0;
            _isWeaponChanging = true;

            if (_weaponCoroutine != null)
            {
                StopCoroutine(_weaponCoroutine);
                _animator.SetBool("IsWeaponChange", false);
            }

            _weaponCoroutine = StartCoroutine(CoChangeWeapon());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (_weaponIndex == 1) return;

            _weaponIndex = _weaponIndex == 0 ? 1 : 0;
            _isWeaponChanging = true;

            if (_weaponCoroutine != null)
            {
                StopCoroutine(_weaponCoroutine);
                _animator.SetBool("IsWeaponChange", false);
            }

            _weaponCoroutine = StartCoroutine(CoChangeWeapon());
        }
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
        if (Input.GetMouseButtonDown(1) && !_isWeaponChanging)
        {
            _isAiming = true;
            _animator.SetBool("IsAiming", true);
            _aimUI.SetActive(true);
            _isZooming = true;
        }
        else if (Input.GetMouseButtonUp(1) && !_isWeaponChanging)
        {
            _isAiming = false;
            _animator.SetBool("IsAiming", false);
            _aimUI.SetActive(false);
            _isZooming = true;
        }

        if (!_isZooming)
            return;

        Transform target = _isAiming ? _zoomCamera.transform : _mainCamera.transform;
        float targetFOV = _isAiming ? _zoomCamera.fieldOfView : _mainCamera.fieldOfView;
        Camera current = Camera.main;

        current.transform.position = Vector3.Lerp(current.transform.position, target.position, _transitionSpeed * Time.deltaTime);
        current.transform.rotation = Quaternion.Slerp(current.transform.rotation, target.rotation, _transitionSpeed * Time.deltaTime);
        current.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, _transitionSpeed * Time.deltaTime);

        if (Vector3.Distance(current.transform.position, target.position) < 0.01f && Quaternion.Angle(current.transform.rotation, target.rotation) < 0.1f)
        {
            Debug.Log("Zoom transition complete.");
            current.transform.position = target.position;
            current.transform.rotation = target.rotation;
            current.fieldOfView = targetFOV;
            _isZooming = false;
        }
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

    IEnumerator CoChangeWeapon()
    {
        yield return new WaitForSeconds(0.03f);

        _animator.SetBool("IsWeaponChange", true);

        yield return new WaitForSeconds(0.9f);

        OnWeaponChange();

        yield return new WaitForSeconds(0.1f);

        _animator.SetBool("IsWeaponChange", false);
        _isWeaponChanging = false;
        _weaponCoroutine = null;
    }

    #region MyRegion

    public void TakeDamage(int damage)
    {
        Debug.Log($"TAKE DAMAGE {damage}");
        hp -= damage;
    }

    #endregion
}
