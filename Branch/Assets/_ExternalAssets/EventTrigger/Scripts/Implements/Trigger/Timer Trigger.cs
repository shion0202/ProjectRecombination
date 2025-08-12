using System.Collections;
using UnityEngine;

public class TimerTrigger : BaseEventTrigger
{
    [Header("타이머 지속 시간")]
    [Range(1, 999)] public int duration = 10;

    // 공개적으로 남은 시간을 확인할 수 있도록 프로퍼티 추가
    public float RemainingTime { get; private set; }
    public bool IsRunning { get; private set; }

    private Coroutine timerCoroutine;

    // Execute가 호출되면 타이머 시작
    public new void Execute(GameObject requester)
    {
        if (IsRunning) return;
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        IsRunning = true;
        RemainingTime = duration;

        while (RemainingTime > 0)
        {
            RemainingTime -= Time.deltaTime;
            yield return null;
        }

        RemainingTime = 0;
        IsRunning = false;

        // 시간이 다 되면 베이스의 Execute를 호출해 이벤트 실행
        base.Execute(this.gameObject);
        timerCoroutine = null;
    }
}