using UnityEngine;

[System.Serializable]
public class EventdataDeactivateGameObject : EventData
{
    [Tooltip("활성화할 오브젝트")]
    public GameObject targetGameObject;
    [Tooltip("활성화 지연 시간 (초)")]
    public float activationDelay;

    private void Start()
    {
        targetGameObject.SetActive(true);
    }

    public override void Execute(GameObject requester)
    {
        if (activationDelay > 0)
        {
            requester.GetComponent<MonoBehaviour>().Invoke(nameof(DeactivateObject), activationDelay);
        }
        else
        {
            DeactivateObject();
        }
    }

    private void DeactivateObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(false);
            Debug.Log(targetGameObject.name + " 오브젝트가 비활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("비활성화할 GameObject가 설정되지 않았습니다.");
        }
    }
}