using System.Collections.Generic;
using UnityEngine;

public class StayTrigger : BaseEventTrigger
{
    [Header("트리거 대상 태그")]
    [Tooltip("'Untagged'로 두면 모든 대상에 반응합니다.")]
    [SerializeField] private string triggerTag = "Player";

    // 트리거 내부에 머무는 객체들을 관리하는 리스트
    private readonly List<GameObject> objectsInTrigger = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (triggerTag == "Untagged" || other.CompareTag(triggerTag))
        {
            // 리스트에 없으면 추가
            if (!objectsInTrigger.Contains(other.gameObject))
            {
                objectsInTrigger.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 리스트에 있으면 제거
        if (objectsInTrigger.Contains(other.gameObject))
        {
            objectsInTrigger.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        // 리스트에 객체가 하나라도 있을 경우에만 이벤트 실행
        if (objectsInTrigger.Count > 0)
        {
            // 리스트의 첫 번째 객체를 기준으로 이벤트 실행
            // 여러 객체가 동시에 들어올 경우의 로직은 기획에 따라 변경될 수 있음
            Execute(objectsInTrigger[0]);
        }
    }
}