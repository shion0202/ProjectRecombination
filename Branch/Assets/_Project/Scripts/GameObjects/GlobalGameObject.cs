using UnityEngine;
using System;

public class GlobalGameObject : MonoBehaviour
{
    public event Action OnObjectDestroyed;
    
    private void OnDestroy()
    {
        // 오브젝트가 파괴되었음을 외부에 알림
        Debug.Log(gameObject.name + "is Destroy");

        OnObjectDestroyed?.Invoke();
    }
}