using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    // 풀링 오브젝트일 때와 아닐 때를 구분하여 코드를 작성하는 것이 번거롭기에 구현한 범용 Instantiate 함수
    // 추후에 풀링 오브젝트 여부가 바뀌더라도 큰 수정 없이 변경 가능
    public static GameObject Instantiate(GameObject go)
    {
        // 풀링 오브젝트일 경우
        if (PoolManager.Instance.IsPooledObject(go).Item1)
        {
            GameObject pooledObject = PoolManager.Instance.GetObject(go);
            return pooledObject;
        }

        return GameObject.Instantiate(go);
    }

    public static GameObject Instantiate(GameObject go, Transform parent)
    {
        if (PoolManager.Instance.IsPooledObject(go).Item1)
        {
            GameObject pooledObject = PoolManager.Instance.GetObject(go, parent);
            return pooledObject;
        }

        return GameObject.Instantiate(go, parent);
    }

    public static GameObject Instantiate(GameObject go, Vector3 position, Quaternion rotation)
    {
        if (PoolManager.Instance.IsPooledObject(go).Item1)
        {
            GameObject pooledObject = PoolManager.Instance.GetObject(go, position, rotation);
            return pooledObject;
        }

        return GameObject.Instantiate(go, position, rotation);
    }

    public static GameObject Instantiate(GameObject go, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (PoolManager.Instance.IsPooledObject(go).Item1)
        {
            GameObject pooledObject = PoolManager.Instance.GetObject(go, position, rotation, parent);
            return pooledObject;
        }

        return GameObject.Instantiate(go, position, rotation, parent);
    }

    public static void Destroy(GameObject go, float delay = 0.0f)
    {
        if (PoolManager.Instance.IsPooledObject(go).Item1)
        {
            PoolManager.Instance.ReleaseObject(go, delay);
            return;
        }

        GameObject.Destroy(go, delay);
    }
}
