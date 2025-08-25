using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] private float explosionForce = 500.0f;
    [SerializeField] private Vector3 explosionPosition;
    [SerializeField] private float explosionRadius = 5.0f;
    [SerializeField] private float upwardsModifier = 0.0f;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();
    private List<Rigidbody> rigidbodies = new List<Rigidbody>();
    private Coroutine Coroutine = null;

    private void Start()
    {
        // 초기 위치, 회전, Rigidbody 저장
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rigidbodies.Add(rb);
                originalPositions.Add(child.position);
                originalRotations.Add(child.rotation);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            BreakWall();
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            ResetWall();
        }
    }

    public void BreakWall()
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position + explosionPosition, explosionRadius, upwardsModifier, forceMode);
            }

            MeshCollider mc = child.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.excludeLayers |= (1 << LayerMask.NameToLayer("Player"));
            }
        }

        if (Coroutine != null)
        {
            StopCoroutine(Coroutine);
        }
        Coroutine = StartCoroutine(CoWallDisable());
    }

    public void ResetWall()
    {
        if (Coroutine != null)
        {
            StopCoroutine(Coroutine);
        }

        for (int i = 0; i < rigidbodies.Count; i++)
        {
            Rigidbody rb = rigidbodies[i];
            rb.isKinematic = true; // 물리 일시 정지
            rb.gameObject.SetActive(true);

            rb.transform.position = originalPositions[i];
            rb.transform.rotation = originalRotations[i];

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = false; // 물리 다시 활성화

            MeshCollider mc = rb.gameObject.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.excludeLayers = 0; // 모든 레이어와 충돌
            }
        }
    }

    private IEnumerator CoWallDisable()
    {
        yield return new WaitForSeconds(5.0f);

        for (int i = 0; i < rigidbodies.Count; i++)
        {
            Rigidbody rb = rigidbodies[i];
            rb.gameObject.SetActive(false);
        }
    }
}
