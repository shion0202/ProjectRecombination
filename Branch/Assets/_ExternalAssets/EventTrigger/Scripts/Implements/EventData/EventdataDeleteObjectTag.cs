using System.Diagnostics.Tracing;
using UnityEngine;
public class EventdataDeleteObjectTag : EventData
{
    [Header("이벤트시 삭제 할 대상")]
    public GameObject objDelete;
    [Header("이벤트를 발동시킬 대상의 태그 (none:대상의 태그와 상관없이 발동)")]
    public string tagInvoker = "none";
    public void DeleteThisObject()
    {
        if (objDelete != null)
        {
            Destroy(objDelete);
            Debug.Log(objDelete.name + " 오브젝트가 삭제되었습니다.");
        }
        else
        {
            Debug.LogError("삭제할 오브젝트가 할당되지 않았습니다.");
        }
    }
    public override void Execute(GameObject eventInvoker)
    {
        // 이벤트가 발생했을 때 오브젝트를 삭제하는 로직을 여기에 구현합니다.
        // 예를 들어, Execute()가 호출되면 즉시 삭제하거나, 특정 조건을 만족했을 때 삭제하도록 할 수 있습니다.
        if (tagInvoker == "none")
        {
            if (eventInvoker && eventInvoker.tag == tagInvoker)
                DeleteThisObject();
        }
        else
        {
            DeleteThisObject();
        }
    }
}