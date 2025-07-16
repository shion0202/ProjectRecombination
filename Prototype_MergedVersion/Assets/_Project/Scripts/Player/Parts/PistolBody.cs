using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolBody : MonoBehaviour, IPartAbility
{
    public void UseAbility(PlayerController owner)
    {
        Shoot(owner);
    }

    private void Shoot(PlayerController owner)
    {
        owner.PartShoot(20.0f, 0.05f, 0.2f, 2.0f, 4.0f, Vector3.zero, 10.0f);
    }
}
