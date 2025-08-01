using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FaceTowards : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material faceMaterial;

    private void Update()
    {
        SetHeadDirection();
    }

    private void SetHeadDirection()
    {
        if (targetTransform != null && faceMaterial != null)
        {
            faceMaterial.SetVector("_FaceForwardDirection", targetTransform.forward);
            faceMaterial.SetVector("_FaceRightDirection", targetTransform.right);
        }
    }
}
