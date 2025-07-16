using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleBody : MonoBehaviour, IPartAbility
{
    public void UseAbility(PlayerController owner)
    {
        Shoot(owner);
    }

    private void Shoot(PlayerController owner)
    {
        owner.PartShoot(15.0f, 0.5f, 0.5f, 2.0f, 5.0f, new Vector3(0.01f, 0.01f), 100.0f);
    }
}
