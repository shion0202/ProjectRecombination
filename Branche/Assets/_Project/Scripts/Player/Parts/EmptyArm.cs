using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyArm : PartArmBase
{
    public override void UseAbility()
    {
        Debug.Log("Empty Arm의 공격! 그러나 아무 일도 일어나지 않았다...");
    }
}
