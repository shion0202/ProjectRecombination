using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Follow Camera Data", order = 10)]
public class FollowCameraData : ScriptableObject
{
    [Header("시야각")]
    [Range(1.0f, 179.0f)] public float FOV;

    [Header("카메라 위치")]
    [Range(-0.5f, 1.5f), Tooltip("카메라 기준 캐릭터의 X축 위치")] public float screenX;
    [Range(-0.5f, 1.5f), Tooltip("카메라 기준 캐릭터의 Y축 위치")] public float screenY;
    [Range(0.01f, 10.0f), Tooltip("캐릭터와 카메라 간의 거리")] public float cameraDistance;

    // 카메라가 캐릭터와 완전히 동일하게 움직이는 방식을 벗어난다면 Zone 관련 변수도 필요

    [Header("마우스 감도")]
    [Range(0.01f, 10.0f), Tooltip("X축 감도")] public float sensitivityX;
    [Range(0.01f, 10.0f), Tooltip("Y축 감도")] public float sensitivityY;

    [Header("카메라 회전 범위")]
    [Range(-180.0f, 180.0f), Tooltip("X축(오른쪽) 카메라 회전 범위")] public float maxAimRangeX;
    [Range(-180.0f, 180.0f), Tooltip("X축(왼쪽) 카메라 회전 범위")] public float minAimRangeX;
    [Range(-180.0f, 180.0f), Tooltip("Y축(아래쪽) 카메라 회전 범위")] public float maxAimRangeY;
    [Range(-180.0f, 180.0f), Tooltip("Y축(위쪽) 카메라 회전 범위")] public float minAimRangeY;

    [Header("카메라 가속/감속 시간")]
    [Range(0.0f, 10.0f), Tooltip("카메라 움직임이 최고 속도에 도달하기 위해 필요한 시간\n0을 입력하면 마우스 입력 시 즉시 최고 속도가 됩니다.")] public float accelTimeX;
    [Range(0.0f, 10.0f), Tooltip("카메라 속도가 0이 될 때까지 필요한 시간\n0을 입력하면 마우스 입력이 끝날 시 즉시 멈춥니다.")] public float decelTimeX;
    [Range(0.0f, 10.0f), Tooltip("카메라 움직임이 최고 속도에 도달하기 위해 필요한 시간\n0을 입력하면 마우스 입력 시 즉시 최고 속도가 됩니다.")] public float accelTimeY;
    [Range(0.0f, 10.0f), Tooltip("카메라 속도가 0이 될 때까지 필요한 시간\n0을 입력하면 마우스 입력이 끝날 시 즉시 멈춥니다.")] public float decelTimeY;

    [Header("카메라 상태 전환 속도")]
    [Range(0.01f, 100.0f), Tooltip("다른 카메라 상태에서 현재 카메라 상태로 전환될 때의 카메라가 전환되는 속도")] public float convertSpeed;
}
