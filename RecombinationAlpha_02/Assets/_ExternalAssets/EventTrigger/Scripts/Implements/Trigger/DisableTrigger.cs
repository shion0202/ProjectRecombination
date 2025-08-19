using UnityEngine;

public class DisableTrigger : BaseEventTrigger
{
    private void OnDisable()
    {
        // OnDisable에서는 코루틴을 사용하지 않는 즉시 실행 함수를 호출
        ExecuteImmediately(this.gameObject);
    }
}