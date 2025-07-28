using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleBody : PartBase
{
    public override void FinishActionForced()
    {
        
    }

    public override void UseAbility()
    {
        Shoot();
    }

    private void Shoot()
    {
        // _owner.PartShoot(15.0f, 0.5f, 1.0f, 4.0f, 10.0f, new Vector3(0.1f, 0.1f), 100.0f);
    }
}
