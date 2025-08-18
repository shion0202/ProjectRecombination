using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Monster
{
    public abstract class MonsterBase : MonoBehaviour
    {
        [Header("기획자용 설정값")]
        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private CombatSettings combatSettings;
        
        [Header("Monster Default Settings")]
        [SerializeField] private MonsterState state = MonsterState.Spawn;
        [SerializeField] private int currentHealth;
        [SerializeField] private Transform target;
        [SerializeField] private NavMeshAgent agent;
        
        [Header("Animator")]
        [SerializeField] private Animator animator;
        
        [Header("Physics")]
        [SerializeField] protected Rigidbody rigid;
        [SerializeField] private Collider coll;
        
        #region Properties
        
        protected MonsterState State
        {
            get => state;
            set => state = value;
        }
        protected float RunSpeed => movementSettings.runSpeed;
        protected float WalkSpeed => movementSettings.walkSpeed;
        protected float LookAtRange => combatSettings.lookAtRange;
        protected float AttackRange => combatSettings.attackRange;
        protected float AttackCooldown => combatSettings.attackCooldown;
        protected int MaxHealth => combatSettings.maxHealth;
        protected int CurrentHealth => currentHealth;
        protected int Damage => combatSettings.damage;
        protected Transform Target => target;
        protected NavMeshAgent Agent => agent;
        protected Animator Animator => animator;
        
        #endregion
        
        #region Unity Methods
        
        private void Start()
        {
            currentHealth = combatSettings.maxHealth;
            
            if (agent is null)
            {
                Debug.LogWarning("NavMeshAgent component is missing on this GameObject.");
                agent = GetComponent<NavMeshAgent>();
            }
        
            // _animator = GetComponent<Animator>();
            if (animator is null)
            {
                Debug.LogWarning("Animator component is missing on this GameObject.");
                animator = GetComponent<Animator>();
            }
            
            // 객체 인스턴스 후 전투 대상을 플레이어로 지정
            target = MonsterManager.instance.Player.transform;
        }
        
        private void Update()
        {
            
            switch (state)
            {
                case MonsterState.Spawn:    // 6 순위
                    // Handle spawn logic
                    StartCoroutine(nameof(Spawn));
                    break;
                case MonsterState.Idle:     // 4 순위
                    // Handle idle logic
                    Idle();
                    break;
                case MonsterState.Chase:    // 3 순위
                    // Handle chase logic   // 보스, 엘리트 몬스터의 위빙은 여기서 처리
                    Chase();
                    break;
                case MonsterState.Patrol:   // 5 순위
                    // Handle patrol logic
                    Patrol();
                    break;
                case MonsterState.Attack:   // 2 순위
                    // Handle attack logic
                    Attack();
                    break;
                case MonsterState.Hit:      // 1 순위
                    // Handle hit logic
                    break;
                case MonsterState.Dead:     // 0 순위
                    // Handle dead logic
                    Dead();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        #endregion
        
        #region Coroutines
        
        private IEnumerator Spawn()
        {
            yield return new WaitForSecondsRealtime(1f);
            state = MonsterState.Idle;
        }
        
        #endregion
        
        #region Abstract Methods
        
        protected abstract void Idle();
        protected abstract void Chase();
        protected abstract void Attack();
        protected abstract void Dead();
        protected abstract void Patrol();
        
        #endregion
        
        #region Protected Methods
        
        protected void DisablePhysics()
        {
            rigid.isKinematic = false;
            coll.enabled = false;
        }
        
        #endregion
        
        #region Public Methods
        
        public void TakeDamage(int takeDamage)
        {
            currentHealth -= takeDamage;
            if (currentHealth <= 0)
            {
                state = MonsterState.Dead;
            }
        }
        
        public void SetTarget(GameObject player)
        {
            target = player.transform;
        }
        
        public MonsterState GetMonsterState()
        {
            return state;
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, combatSettings.lookAtRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, combatSettings.attackRange);
        }
        
        #endregion
    }
}