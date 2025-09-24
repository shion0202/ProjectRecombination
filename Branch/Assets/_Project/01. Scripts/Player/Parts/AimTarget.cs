using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimTarget : MonoBehaviour
{
    public Transform characterTransform;
    public Transform targetObject;
    public List<MultiAimConstraint> multiAimConstraints;

    [Range(0, 180)] public float maxYawAngle = 135f;
    public float weightChangeSpeed = 2f;  // weight 변화 속도 (1초에 달성)

    private float currentWeight = 1f;

    void LateUpdate()
    {
        if (characterTransform == null || targetObject == null || multiAimConstraints.Count <= 0)
            return;

        // 타겟 로컬 좌표 계산
        Vector3 localTargetPos = characterTransform.InverseTransformPoint(targetObject.position);
        float targetAngle = Mathf.Atan2(localTargetPos.x, localTargetPos.z) * Mathf.Rad2Deg;

        // 제한 각도 넘으면 weight 줄이고, 아니면 늘림
        if (targetAngle > maxYawAngle || targetAngle < -maxYawAngle)
        {
            currentWeight -= weightChangeSpeed * Time.deltaTime;
        }
        else
        {
            currentWeight += weightChangeSpeed * Time.deltaTime;
        }

        // 0~1 clamp
        currentWeight = Mathf.Clamp01(currentWeight);

        // Multi Aim Constraint에 weight 반영
        foreach (var constraint in multiAimConstraints)
        {
            constraint.weight = currentWeight;
        }
        
    }
}
