using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverSlowerCharacter : MonoBehaviour
{
    public Transform dummyTransform;
    public float followPositionSpeed = 5f;
    public float maxTurnAnglePerSec = 180f; // 초당 최대 선회각(에 따라 더 선회가 둥글어진다)

    private CharacterController controller;
    private Vector3 currentMoveDir;         // 내가 실제 이동하는 방향

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // 초기 방향(캐릭터 forward)
        currentMoveDir = transform.forward;
    }

    void Update()
    {
        // 1. 목표 방향: 더미와 자신의 위치 차이
        Vector3 targetDir = (dummyTransform.position - transform.position).normalized;
        if (targetDir.magnitude < 0.01f) targetDir = transform.forward; // 멈출 때 처리

        // 2. 이동 방향을 관성있게 따라가기 (RotateTowards로 선회)
        float maxRadian = maxTurnAnglePerSec * Mathf.Deg2Rad * Time.deltaTime;
        currentMoveDir = Vector3.RotateTowards(currentMoveDir, targetDir, maxRadian, 0f);
        currentMoveDir.Normalize();

        // 3. 위치 이동(Lerp로 보간)
        Vector3 targetPos = dummyTransform.position;
        Vector3 newPos = Vector3.Lerp(transform.position, targetPos, followPositionSpeed * Time.deltaTime);
        Vector3 moveDelta = newPos - transform.position;

        // 4. 캐릭터컨트롤러로 이동 적용, 방향은 관성 방향으로만
        controller.Move(currentMoveDir * moveDelta.magnitude);

        // 5. 회전은 내 이동방향을 바라보게
        if (currentMoveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentMoveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, maxTurnAnglePerSec * Time.deltaTime);
        }
    }
}