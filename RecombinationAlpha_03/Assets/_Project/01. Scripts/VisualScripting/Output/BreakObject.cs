using _Project.Scripts.VisualScripting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BreakableData
{
    public float explosionForce;
    public Vector3 explosionPosition;
    public float explosionRadius;
    public float upwardsModifier;
    public ForceMode forceMode;
}

public class BreakObject : ProcessBase
{
    [SerializeField] private GameObject breakableObject;
    [SerializeField] private BreakableData data;
    private Coroutine Coroutine = null;

    public override void Execute()
    {
        if (IsOn) return;
        if (breakableObject == null) return;

        BreakWall();
    }

    public void BreakWall()
    {
        foreach (Transform child in breakableObject.transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(data.explosionForce, child.position + data.explosionPosition, data.explosionRadius, data.upwardsModifier, data.forceMode);
            }

            MeshCollider mc = child.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.excludeLayers |= (1 << LayerMask.NameToLayer("Player"));
            }
        }

        if (Coroutine == null)
        {
            Coroutine = StartCoroutine(CoWallDisable());
        }
    }

    private IEnumerator CoWallDisable()
    {
        yield return new WaitForSeconds(5.0f);

        foreach (Transform child in breakableObject.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
