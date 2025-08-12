using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCharacter : MonoBehaviour
{
    public float groundY = 0.0f;
    public float hoverHeight = 0.5f;
    public float hoverRange = 0.2f;
    public float hoverSpeed = 2.0f;

    public Animator animator;
    public CharacterController controller;
    private Vector3 basePosition;

    private bool isIdle = true;
    private bool isIdleFrame = false;

    private float hoverStartTime;
    private float hoverInitialY;
    private float hoverPhaseOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        basePosition = transform.position;
    }

    void Update()
    {
        // 캐릭터의 위치는 컨트롤러를 통해 변화시켜야 함
        Vector3 moveDelta = Vector3.zero;

        Vector3 moveXZ = Input.GetAxisRaw("Horizontal") * Vector3.right + Input.GetAxisRaw("Vertical") * Vector3.forward;
        moveXZ.y = 0;
        moveDelta = moveXZ.normalized * Time.deltaTime * 5.0f;

        if (moveXZ.sqrMagnitude > 0)
        {
            isIdle = false;
        }
        else
        {
            isIdle = true;
        }

        if (!isIdleFrame && isIdle)
        {
            hoverStartTime = Time.time;
            hoverInitialY = transform.position.y;

            float currentOffset = transform.position.y - (groundY + hoverHeight);
            float theta = Mathf.Asin(Mathf.Clamp(currentOffset / hoverRange, -1f, 1f));

            // 현재 이동 방향(위/아래) 판별
            float velocityY = (transform.position.y - hoverInitialY) / Time.deltaTime;
            if (velocityY < 0) // 내려가는 중이면
            {
                theta = Mathf.PI - theta;
            }

            // phase offset 계산
            hoverPhaseOffset = theta - (Time.time * hoverSpeed);
        }

        if (isIdle)
        {
            float offsetY = Mathf.Sin(Time.time * hoverSpeed + hoverPhaseOffset) * hoverRange;
            // 기존 위치에서 Y축만 움직임 적용
            float targetY = groundY + hoverHeight + offsetY;
            moveDelta.y = targetY - (transform.position.y);
        }

        // 최종 이동
        controller.Move(moveDelta);

        isIdleFrame = isIdle;
    }
}
