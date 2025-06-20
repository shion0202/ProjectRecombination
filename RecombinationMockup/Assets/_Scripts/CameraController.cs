using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerBody;

    [SerializeField] private float minAngles = 0f;
    [SerializeField] private float maxAngles = 0f;

    private float _xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, minAngles, maxAngles);

        // 카메라 자체 회전
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    
        // 캐릭터 몸통 회전
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
