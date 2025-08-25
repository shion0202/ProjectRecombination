using AI;
using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLaserMultiple : PartBaseArm
{
    protected override void Update()
    {
        _currentShootTime -= Time.deltaTime;
        if (!_isShooting) return;

        if (_currentShootTime <= 0.0f)
        {
            Shoot();
            _currentShootTime = (_owner.Stats.CombinedPartStats[partType][EStatType.IntervalBetweenShots].value);
        }
    }
}
