using UnityEngine;

public class PositionTrigger : BaseEventTrigger
{
    [Header("목표 위치")]
    public Vector3 targetPosition;

    [Header("허용 오차 거리")]
    public float tolerance = 0.1f;

    private void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) <= tolerance)
        {
            Execute(this.gameObject);
            // 한 번 실행 후 비활성화하여 반복적인 Update 호출 방지
            this.enabled = false;
        }
    }
}