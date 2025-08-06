using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLine : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;
    public LineRenderer beamRenderer;

    public float upOffset = 2.0f;
    public float maxSideOffset = 5.0f; // 곡선 세기의 최대값

    public float crosshairRadius = 0.07f; // 0.07 = 화면의 7% 반경(조절 가능)
    public string enemyTag = "Enemy";

    private Transform currentTarget = null;

    void Update()
    {
        // 1. 타겟 해제 조건 확인(마우스 입력 해제/타겟 아웃/죽음 등)
        if (currentTarget != null)
        {
            bool outOfScreen = IsTargetOutOfScreen(currentTarget);
            bool destroyed = !currentTarget.gameObject.activeInHierarchy; // 또는 HP, disable 등 구체화
            bool inputUp = Input.GetMouseButtonUp(0); // 마우스 왼쪽 버튼 해제 시

            if (outOfScreen || destroyed || inputUp)
                currentTarget = null;
        }

        // 2. 타겟이 없다면 크로스헤어 내에서 가장 가까운 적 탐색
        if (currentTarget == null)
        {
            currentTarget = FindClosestEnemyWithinCrosshair();
        }

        // 3. 곡선 빔 그리기 (타겟이 유효할 때)
        if (currentTarget != null)
        {
            DrawCurvedLaser(currentTarget.position);
        }
        else
        {
            // 빔 비활성화 등(옵션)
            beamRenderer.positionCount = 0;
        }

        //targetPoint = FindClosestEnemyWithinCrosshair();
        //if (targetPoint == null)
        //{
        //    beamRenderer.enabled = false;
        //    return;
        //}

        //Vector3 p0 = startPoint.position + Vector3.up;
        //Vector3 p2 = targetPoint.position + Vector3.up;

        //Vector3 cameraRight = Camera.main.transform.right;
        //Vector3 dirToTarget = (p2 - Camera.main.transform.position).normalized;

        //// 1. 카메라 방향에서 타겟이 오른쪽에 있으면 +, 왼쪽이면 - (좌우 연속 값)
        //float sideAmt = Vector3.Dot(dirToTarget, cameraRight);
        //// sideAmt 값이 -1~+1에서 연속적으로 변함

        //// 2. 최대 휘어짐 값 곱해서 적용 (비례해서 자연스럽게 곡선 변화)
        //float appliedSideOffset = maxSideOffset * -sideAmt;

        //// 3. midPoint 계산
        //Vector3 midPoint = (p0 + p2) * 0.5f + Vector3.up * upOffset + cameraRight * appliedSideOffset;

        //// 4. 베지어 곡선 샘플링
        //int segmentCount = 20;
        //beamRenderer.positionCount = segmentCount + 1;
        //for (int i = 0; i <= segmentCount; i++)
        //{
        //    float t = i / (float)segmentCount;
        //    Vector3 bezierPoint =
        //        Mathf.Pow(1 - t, 2) * p0
        //        + 2 * (1 - t) * t * midPoint
        //        + Mathf.Pow(t, 2) * p2;
        //    beamRenderer.SetPosition(i, bezierPoint);
        //}

        //beamRenderer.enabled = true;
    }

    // 크로스헤어 안에서 가장 가까운 적 반환
    Transform FindClosestEnemyWithinCrosshair()
    {
        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float minScreenDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.transform.position);
            if (viewportPos.z > 0)
            {
                Vector2 center = new Vector2(0.5f, 0.5f);
                Vector2 enemyXY = new Vector2(viewportPos.x, viewportPos.y);
                float dist = Vector2.Distance(center, enemyXY);

                if (dist < crosshairRadius && dist < minScreenDist)
                {
                    // Raycast로 시야 가려짐 검증
                    Vector3 dir = enemy.transform.position - Camera.main.transform.position;
                    if (Physics.Raycast(Camera.main.transform.position, dir.normalized, out RaycastHit hit, 100f))
                    {
                        // 적 본체 또는 자식 오브젝트가 맞았을 때만 인정
                        if (hit.collider != null &&
                            (hit.collider.gameObject == enemy.gameObject ||
                             hit.collider.transform.IsChildOf(enemy.transform)))
                        {
                            closest = enemy.transform;
                            minScreenDist = dist;
                        }
                    }
                }
            }
        }
        return closest;

        //var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        //Transform closest = null;
        //float minScreenDist = float.MaxValue;

        //foreach (var enemy in enemies)
        //{
        //    // 1. 적의 월드 좌표 → 뷰포트 좌표 (0~1)
        //    Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.transform.position);

        //    // 2. 화면 앞에 있고(0<z), 정중앙으로부터 일정 거리 안이면 후보
        //    if (viewportPos.z > 0)
        //    {
        //        Vector2 screenCenter = new Vector2(0.5f, 0.5f);
        //        Vector2 enemyXY = new Vector2(viewportPos.x, viewportPos.y);
        //        float dist = Vector2.Distance(screenCenter, enemyXY);

        //        if (dist < crosshairRadius && dist < minScreenDist)
        //        {
        //            // (옵션) Raycast로 실제 가려진 적은 제외
        //            Vector3 dir = enemy.transform.position - Camera.main.transform.position;
        //            if (Physics.Raycast(Camera.main.transform.position, dir.normalized, out RaycastHit hit, 100f))
        //            {
        //                // 적 본체 또는 자식 여부 체크
        //                if (hit.collider != null && (hit.collider.gameObject == enemy.gameObject || hit.collider.transform.IsChildOf(enemy.transform)))
        //                {
        //                    closest = enemy.transform;
        //                    minScreenDist = dist;
        //                }
        //            }
        //        }
        //    }
        //}

        //return closest;
    }

    // 타겟이 화면 범위 내에 있는지 확인 (예시)
    bool IsTargetOutOfScreen(Transform target)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(target.position);
        // 화면 앞에 있고, 0~1 사이에 모두 포함돼 있으면 true/false 반환
        return (vp.z < 0 || vp.x < 0 || vp.x > 1 || vp.y < 0 || vp.y > 1);
    }

    // 곡선(베지어) 빔 연출
    void DrawCurvedLaser(Vector3 targetPos)
    {
        Vector3 p0 = startPoint.position + Vector3.up * 0.5f;
        Vector3 p2 = targetPos + Vector3.up * 0.5f;

        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 dirToTarget = (p2 - Camera.main.transform.position).normalized;
        float sideAmt = Vector3.Dot(dirToTarget, cameraRight);
        float appliedSideOffset = maxSideOffset * -sideAmt; // (왼쪽-오른쪽 자연스러운 방향)

        Vector3 midPoint = (p0 + p2) * 0.5f + Vector3.up * upOffset + cameraRight * appliedSideOffset;

        int segmentCount = 20;
        beamRenderer.positionCount = segmentCount + 1;
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 bezierPoint =
                Mathf.Pow(1 - t, 2) * p0
                + 2 * (1 - t) * t * midPoint
                + Mathf.Pow(t, 2) * p2;
            beamRenderer.SetPosition(i, bezierPoint);
        }
    }
}
