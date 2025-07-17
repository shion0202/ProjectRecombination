using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolBody : MonoBehaviour, IPartAbility
{
    [SerializeField, Range(10.0f, 90.0f)] private float bulletSpeed = 50.0f;

    public void UseAbility(PlayerController owner)
    {
        Shoot(owner);
    }

    private void Shoot(PlayerController owner)
    {
        owner.PartShoot(bulletSpeed, 0.05f, 0.2f, 2.0f, 4.0f, Vector3.zero, 10.0f);
    }
}
