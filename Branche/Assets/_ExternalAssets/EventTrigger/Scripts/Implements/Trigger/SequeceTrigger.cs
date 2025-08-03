using System.Collections;
using UnityEngine;

public class SequenceTrigger : BaseEventTrigger
{
    // BaseEventTrigger의 Execute를 오버라이드하여 순차 실행 로직으로 변경
    public new void Execute(GameObject requester)
    {
        StartCoroutine(ExecuteSequence(requester));
    }

    private IEnumerator ExecuteSequence(GameObject requester)
    {
        // isLoop와 hasTriggered 로직은 기획에 따라 추가 가능

        foreach (var eventItem in eventsToExecute)
        {
            if (eventItem.eventData != null)
            {
                // 이전 코루틴이 끝날 때까지 기다림
                // EventData의 Execute가 코루틴을 반환하도록 수정이 필요할 수 있으나,
                // 여기서는 간단히 지연 시간만큼만 대기하는 방식으로 구현
                if (eventItem.delay > 0)
                {
                    yield return new WaitForSeconds(eventItem.delay);
                }

                // 이벤트 실행
                eventItem.eventData.Execute(requester);

                // FadeOutAction의 duration 만큼 기다려주는 로직
                if (eventItem.eventData is FadeOutAction fadeOut)
                {
                    yield return new WaitForSeconds(fadeOut.duration);
                }
            }
        }
    }
}