using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionTest : MonoBehaviour
{
    public GameObject explosionEffectPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.CompareTag("Player") && !other.transform.CompareTag("Bullet"))
        {
            Explode(gameObject);
        }
    }

    void Explode(GameObject missile)
    {
        if (explosionEffectPrefab)
            Instantiate(explosionEffectPrefab, missile.transform.position, Quaternion.identity);

        Destroy(missile);
    }
}
