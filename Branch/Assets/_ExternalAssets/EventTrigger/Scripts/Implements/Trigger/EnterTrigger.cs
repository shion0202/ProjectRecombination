using UnityEngine;

public class EnterTrigger : BaseEventTrigger
{
    [Header("트리거 대상 태그")]
    [Tooltip("'Untagged'로 두면 모든 대상에 반응합니다.")]
    [SerializeField] private string triggerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (triggerTag == "Untagged" || other.CompareTag(triggerTag))
        {
            Execute(other.gameObject);
        }
    }
}