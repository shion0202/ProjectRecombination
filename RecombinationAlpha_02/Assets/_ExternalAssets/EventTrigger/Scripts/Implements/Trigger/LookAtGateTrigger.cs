using UnityEngine;

// EventData를 상속받는 ScriptableObject로 만드는 것을 추천
[CreateAssetMenu(fileName = "New LookAtTarget Event", menuName = "Events/Look At Target")]
public class LookAtTarget : EventData
{
    public Transform targetToLookAt;

    public override void Execute(GameObject requester)
    {
        if (requester != null && targetToLookAt != null)
        {
            // requester가 targetToLookAt을 바라보도록 함 (Y축은 requester의 원래 높이 유지)
            Vector3 targetPosition = new Vector3(targetToLookAt.position.x, requester.transform.position.y, targetToLookAt.position.z);
            requester.transform.LookAt(targetPosition);
        }
    }
}