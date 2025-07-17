using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Follow Camera Data", order = 0)]
public class FollowCameraData : ScriptableObject
{
    public float FOV;

    public float screenX;
    public float screenY;
    public float cameraDistance;

    public float maxAimRangeX;
    public float minAimRangeX;
    public float maxAimRangeY;
    public float minAimRangeY;
    public float sensitivityX;
    public float sensitivityY;
}
