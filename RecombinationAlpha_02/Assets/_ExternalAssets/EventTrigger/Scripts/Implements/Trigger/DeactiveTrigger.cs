using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactiveTrigger : MonoBehaviour
{
    [Header("대상이 비활성화될 때 실행할 이벤트들")]
    public List<EventData> deactiveEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    private void OnDisable()
    {
        for (int i = 0;i < deactiveEvents.Count;i++)
        {
            if (deactiveEvents[i] != null)
            {
                StartCoroutine(ExecuteEventWithDelay(deactiveEvents[i], delayTimes[i]));
            }
        }
    }


    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }
}
