using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    // 사용하지 않는다.
    [CreateAssetMenu(menuName = "BehaviorTree/Blackboard")]
    public class Blackboard : ScriptableObject
    {
        public NavMeshAgent navMeshAgent;
        public GameObject agent;        // AI 에이전트 오브젝트
        public Transform target;        // 타겟 오브젝트
        
        public float detectionRange;    // 타겟 인식 범위
        public float health;            // AI의 현재 체력
        public float maxHealth;         // AI의 최대 체력
        
        // AI의 상태를 나타내는 변수들
    }
}