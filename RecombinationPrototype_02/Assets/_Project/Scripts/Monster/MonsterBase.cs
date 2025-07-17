using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Jaeho.Monster
{
    [Serializable]
    public class MovementSettings
    {
        public float minMoveSpeed = 2f;
        public float maxMoveSpeed = 5f;
    }

    [Serializable]
    public class CombatSettings
    {
        public float lookAtRange = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
        public int maxHealth = 100;
        public int damage = 10;
    }
    
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

        #region Properties

        protected MonsterState State
        {
            get => state;
            set => state = value;
        }
        protected float MaxMoveSpeed => movementSettings.maxMoveSpeed;
        protected float MinMoveSpeed => movementSettings.minMoveSpeed;
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
        }
        
        private void Update()
        {
            // if (_target is not null) return;
            // SetTarget();
            
            switch (state)
            {
                case MonsterState.Spawn:
                    // Handle spawn logic
                    StartCoroutine(nameof(Spawn));
                    break;
                case MonsterState.Idle:
                    // Handle idle logic
                    Idle();
                    break;
                case MonsterState.Chase:
                    // Handle chase logic
                    Chase();
                    break;
                case MonsterState.Patrol:
                    // Handle patrol logic
                    break;
                case MonsterState.Attack:
                    // Handle attack logic
                    Attack();
                    break;
                // case MonsterState.Hit:
                //     // Handle hit logic
                //     break;
                case MonsterState.Dead:
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

        #endregion

        #region Default Methods

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

        #endregion

        #region Public Methods

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