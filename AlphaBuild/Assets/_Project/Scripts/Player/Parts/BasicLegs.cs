using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicLegs : PartBase
{
    public override void UseAbility(PlayerController owner)
    {
        Dash(owner);
    }

    private void Dash(PlayerController owner)
    {
        owner.PartDash();
    }
}
