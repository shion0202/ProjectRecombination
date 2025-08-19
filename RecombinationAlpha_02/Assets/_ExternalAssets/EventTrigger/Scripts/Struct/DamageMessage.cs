using UnityEngine;

public struct DamageMessage
{
    public GameObject damager;
    public float amount;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
    public char color;// 'W' 또는 'B'
    public float value;// 스택 값
}