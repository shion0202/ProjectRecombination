using System;
using UnityEngine;
using UnityEngine.AI;

public enum MonsterState
{
    Spawn = 0,
    Idle = 1,
    Chase = 2,
    Patrol = 3,
    Attack = 4,
    Hit = 5,
    Dead = 6,
}

[Serializable]
public class MonsterStats : MonoBehaviour
{
    [SerializeField] private MonsterState state = MonsterState.Idle;
    
    [Header("Components")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private GameObject agent;
    [SerializeField] private GameObject target;
    
    [Header("Monster Stats")]
    [SerializeField] private float health = 100f;        // 현재 체력
    [SerializeField] private float maxHealth = 100f;     // 최대 체력
    [SerializeField] private float detectionRange = 10f; // 타겟 인식 범위
    
    [Header("Attack 1 Stats")]
    [SerializeField] private float attack1Range = 2f;     // 공격 범위
    [SerializeField] private float attack1Damage = 10f;   // 공격력
    [SerializeField] private float attack1Cooldown = 1f;  // 공격 쿨타임
    
    [Header("Attack 2 Stats")]
    [SerializeField] private float attack2Range = 2f;     // 공격 범위
    [SerializeField] private float attack2Damage = 10f;   // 공격력
    [SerializeField] private float attack2Cooldown = 1f;  // 공격 쿨타임
    
    [Header("Movement Stats")]
    [SerializeField] private float speed = 3.5f;         // 이동 속도
    [SerializeField] private float rotationSpeed = 5f;   // 회전 속도
    [SerializeField] private float chaseSpeed = 5f;      // 추적 속도
    [SerializeField] private float fleeSpeed = 7f;       // 도망 속도
    [SerializeField] private float fleeHealthThreshold = 30f; // 도망치는 체력 임계값
    [SerializeField] private float targetDistance;       // 타겟과의 거리
    
    [Header("Patrol Stats")]
    // [SerializeField] private float wanderRadius = 10f;   // 방황 반경
    [SerializeField] private Vector3[] patrolPath;          // 방황 경로 (여러 지점으로 구성된 배열)
    [SerializeField] private float patrolTimer = 5f;        // 방황 타이머
    [SerializeField] private float patrolColTimer = 5f;        // 방황 쿨타임 타이머
    
    // Properties to access the stats
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public GameObject Agent => agent;
    public GameObject Target => target;
    public float Health => health;
    public float MaxHealth => maxHealth;
    public float DetectionRange => detectionRange;
    public float Attack1Range => attack1Range;       // 공격 스킬 1 범위
    public float Attack1Damage => attack1Damage;     // 공격 스킬 1 데미지
    public float Attack1Cooldown => attack1Cooldown; // 공격 스킬 1 쿨타임
    
    public float Attack2Range => attack2Range;       // 공격 스킬 2 범위
    public float Attack2Damage => attack2Damage;     // 공격 스킬 2 데미지
    public float Attack2Cooldown => attack2Cooldown; // 공격 스킬 2 쿨타임
    
    public float Speed => speed;
    public float RotationSpeed => rotationSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float FleeSpeed => fleeSpeed;
    public float FleeHealthThreshold => fleeHealthThreshold;
    public float TargetDistance => targetDistance; 
    
    // public float WanderRadius => wanderRadius;
    public Vector3[] PatrolPath => patrolPath;        // 방황 경로
    public float PatrolTimer => patrolTimer;
    public float PatrolColTimer => patrolColTimer;
    
    public MonsterState State
    {
        get => state;
        set => state = value;
    }
    
    private void Awake()
    {
        
    }

    private void Start()
    {
        MonsterManager.instance.AddMonster(gameObject);
        target = MonsterManager.instance.Player;
    }

    private void Update()
    {
        targetDistance = Vector3.Distance(Agent.transform.position, Target.transform.position);
    }
    
    // Gizmo를 사용하여 몬스터의 인식 범위를 시각적으로 표시
    private void OnDrawGizmosSelected()
    {
        // Gizmos의 색상을 설정
        Gizmos.color = Color.red;
        // 몬스터의 위치를 기준으로 인식 범위를 원으로 표시
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 공격 범위를 원으로 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attack1Range);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attack2Range);
        
        // 방황 반경을 원으로 표시
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        // NavMeshAgent의 경로를 시각적으로 표시
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            // 경로의 각 지점을 선으로 연결
            Gizmos.color = Color.cyan;
            for (int i = 0; i < navMeshAgent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1]);
            }
        }
        
        // PatrolPath의 각 지점을 사각형으로 표시
        if (patrolPath != null && patrolPath.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in patrolPath)
            {
                Gizmos.DrawCube(point, Vector3.one * 0.5f); // 각 지점을 작은 큐브로 표시
                
            }
        }
    }
}