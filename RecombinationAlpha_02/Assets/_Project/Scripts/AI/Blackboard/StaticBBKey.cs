using UnityEngine;

namespace AI.Blackboard
{
    // 정적 키 선언
    public static class StaticBBKey
    {
        [Header("Monster Current Stats")]
        public static readonly BBKey<float> Health = new("Health");
        public static readonly BBKey<float> MinSpeed = new("MinSpeed");
        public static readonly BBKey<float> MaxSpeed = new("MaxSpeed");
        public static readonly BBKey<float> Damage = new("Damage");
        // public static readonly BBKey<float> AttackSpeed = new("AttackSpeed");
        public static readonly BBKey<float> Defence = new("Defence");
        
        [Header("Monster Config")]
        public static readonly BBKey<float> DetectionRange = new("DetectionRange");
        // public static readonly BBKey<float> RotationSpeed = new("RotationSpeed");
    }
}