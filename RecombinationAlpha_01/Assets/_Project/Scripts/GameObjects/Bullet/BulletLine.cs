using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLine : MonoBehaviour
{
    private LineRenderer line;
    private Vector3 previousPosition;

    [SerializeField]
    private float minDistance = 0.1f;
    [SerializeField]
    private float width = 0.1f;
    private void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 1;
        line.startWidth = line.endWidth = width;
        previousPosition = transform.position;
    }


}
