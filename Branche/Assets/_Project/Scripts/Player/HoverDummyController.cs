using UnityEngine;

public class HoverDummyController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // 초당 회전 속도
    public Transform cameraTransform;   // 메인 카메라 Transform

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. 입력
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 입력 없으면 종료
        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude < 0.01f)
            return;

        // 2. 카메라 방향 기반 변환
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // 카메라의 Y축 회전만 반영 (상하 기울임 무시)
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 입력 방향을 카메라 기준으로 재계산
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // 3. 위치 이동
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // 4. 회전 (이동 방향을 바라보도록)
        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    // 현재 더미가 움직이는지 여부를 플레이어가 알 수 있도록
    public bool IsIdle()
    {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).sqrMagnitude < 0.01f;
    }
}