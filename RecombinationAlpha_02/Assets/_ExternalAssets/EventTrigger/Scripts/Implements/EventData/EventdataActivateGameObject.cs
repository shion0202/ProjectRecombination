
using UnityEngine;

[System.Serializable]
public class EventdataActivateGameObject : EventData
{
    [Tooltip("활성화할 오브젝트")]
    public GameObject targetGameObject;

    private void Start()
    {
        targetGameObject.SetActive(false);
    }

    public override void Execute(GameObject requester)
    {
        ActivateObject();
    }

    private void ActivateObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(true);
            Debug.Log(targetGameObject.name + " 오브젝트가 활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("활성화할 GameObject가 설정되지 않았습니다.");
        }
    }
}