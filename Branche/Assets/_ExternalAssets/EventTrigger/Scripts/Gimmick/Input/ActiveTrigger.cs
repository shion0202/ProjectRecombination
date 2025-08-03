using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTrigger : MonoBehaviour
{
    [Header("대상이 활성화될 때 실행할 이벤트들")]
    public List<EventData> activeEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    private void Update()
    {
        if (this.gameObject.activeSelf)
        {
            while (delayTimes.Count < activeEvents.Count)
            {
                delayTimes.Add(0f);
            }
            for (int i = 0;i < activeEvents.Count;i++)
            {
                if (activeEvents[i] != null)
                {
                    StartCoroutine(ExecuteEventWithDelay(activeEvents[i], delayTimes[i]));
                }
            }
            //실행 후 update 비활성화
            enabled = false;
        }
    }
    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute(this.gameObject);
    }
}
