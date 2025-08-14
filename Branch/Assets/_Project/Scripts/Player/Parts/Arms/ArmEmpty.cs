using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmEmpty : PartBaseArm
{
    protected override void Awake()
    {
        base.Awake();
        _isAnimating = false;
    }

    protected override void Update()
    {

    }
}
