using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventWithDelay
{
    public EventData eventData;
    [Tooltip("이 이벤트를 실행하기 전 지연 시간(초)")]
    public float delay;
}

// 모든 트리거의 기반이 될 추상 클래스
public abstract class BaseEventTrigger : MonoBehaviour
{
    [Header("실행할 이벤트 목록")]
    [SerializeField] protected List<EventWithDelay> eventsToExecute;

    [Header("반복 실행 여부")]
    [SerializeField] private bool isLoop = false;

    private bool hasTriggered = false;

    // 코루틴을 사용하는 일반 실행 함수
    protected void Execute(GameObject invoker)
    {
        if (hasTriggered && !isLoop) return;

        foreach (var eventItem in eventsToExecute)
        {
            if (eventItem.eventData != null)
            {
                StartCoroutine(ExecuteWithDelay(eventItem, invoker));
            }
        }
        hasTriggered = true;
    }

    // OnDisable용 즉시 실행 함수
    protected void ExecuteImmediately(GameObject invoker)
    {
        if (hasTriggered && !isLoop) return;

        foreach (var eventItem in eventsToExecute)
        {
            if (eventItem.eventData != null) eventItem.eventData.Execute(invoker);
        }
        hasTriggered = true;
    }

    private IEnumerator ExecuteWithDelay(EventWithDelay eventItem, GameObject invoker)
    {
        if (eventItem.delay > 0)
        {
            yield return new WaitForSeconds(eventItem.delay);
        }
        eventItem.eventData.Execute(invoker);
    }
}