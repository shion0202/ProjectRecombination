using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewToCameraAlways : MonoBehaviour
{
    private Vector3 defaultPos;

    private void Start()
    {
        //defaultPos = transform.localPosition;
        //Vector3 direction = (transform.position - Camera.main.transform.position).normalized;
        //transform.localPosition = defaultPos - direction * 1f + new Vector3(0.0f, 1.0f, 0.0f);

        LookCamera();
    }

    private void Update()
    {
        //Vector3 direction = (transform.position - Camera.main.transform.position).normalized;
        //transform.localPosition = defaultPos - direction * 1f + new Vector3(0.0f, 1.0f, 0.0f);

        LookCamera();
    }

    private void LookCamera()
    {
        Vector3 direction = transform.position - Camera.main.transform.position;
        direction.y = 0; // 수평 회전만 원한다면

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
