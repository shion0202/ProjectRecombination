using System.Collections;
using UnityEngine;

namespace Monster
{
    public class MonsterNormal : MonsterBase
    {
        // [SerializeField] private MonsterPatrol patrol;
        
        private bool _isAttacking;
        private bool _isDead;
        private bool _isPatrol;
        
        #region Default Methods

        protected override void Idle()
        {
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for idle state.");
                return;
            }

            if (_isAttacking) return;
            
            // SetIdleAnima(Random.Range(1, 4));
            SetAnimationState(State, Random.Range(1, 4));
            
            // Idle logic for the normal monster
            
            if (Vector3.Distance(transform.position, Target.position) > LookAtRange) return;
            State = MonsterState.Chase;
        }
        
        protected override void Chase()
        {
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for chasing.");
                return;
            }
            
            if (_isAttacking) return;
            
            // Set Run animation
            // SetRunAnima();
            SetAnimationState(State);
            
            // Chase logic
            Agent.SetDestination(Target.position);
            
            if (!(Vector3.Distance(transform.position, Target.position) <= AttackRange)) return;
            Agent.isStopped = true;
            State = MonsterState.Attack;
        }
        
        protected override void Attack()
        {
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for attacking.");
                return;
            }
            
            if (!_isAttacking)
            {
                // Set Attack animation
                SetAnimationState(State, Random.Range(1, 4));

                // Start attack coroutine
                StartCoroutine(WaitForAttackEnd());
            }

            if (!(Vector3.Distance(transform.position, Target.position) > AttackRange)) return;
            Agent.isStopped = false;
            State = MonsterState.Chase;
        }
        
        protected override void Dead()
        {
            if (Animator is null)
            {
                Debug.LogError("Animator component is missing on this GameObject.");
                return;
            }
            Agent.isStopped = true; // 이동 중인 경우 정지

            if (!_isDead)
            {
                // 캐릭터 사망 처리 시 리지드바디 및 콜라이더 비활성화
                DisablePhysics();
                // 캐릭터 사망 애니메이션 재생
                StartCoroutine(WaitForDeadEnd());
            }
            
        }

        protected override void Patrol()
        {
            // if (patrol is null)
            // {
            //     Debug.LogError("Patrol component is missing on this GameObject.");
            //     return;
            // }
            //
            // if (!_isPatrol)
            // {
            //     StartCoroutine();
            // }
        }
            

        #endregion
        
        #region Animation

        private void SetAnimationState(MonsterState state = MonsterState.Idle, int value = 0)
        {
            if (Animator is null)
            {
                Debug.LogError("Animator component is missing on this GameObject.");
                return;
            }

            var nState = (int)state;

            if (nState is 0 or 5 or 6) return; // "Spawn" state and "Hit" state and "Dead" state does not have an animation
            
            Animator.SetInteger("State", nState);
            
            var keyName = string.Empty;
            
            if (state == MonsterState.Idle)
                keyName = "IdleKey";
            else if (state == MonsterState.Attack)
                keyName = "AttackKey";
            else
                keyName = "";
            
            if (!string.IsNullOrEmpty(keyName))
            {
                Animator.SetInteger(keyName, value);
            }
        }

        private IEnumerator WaitForAttackEnd()
        {
            // Animator 가 null 아니라 가정하에 동작합니다.
            if (Animator is null)
            {
                Debug.LogError("Animator component is missing on this GameObject.");
                yield break;
            }

            _isAttacking = true;
            yield return null;
            
            // 공격 시 캐릭터 방향을 바라보도록 한다.
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for attacking.");
                _isAttacking = false;
                yield break;
            }
            transform.LookAt(Target);
            
            
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            var length = stateInfo.length;
            
            // TODO: Develop Attack logic
            Debug.Log($"Attacking target: {Target.name} with damage: {Damage}");
            
            yield return new WaitForSeconds(length);
            _isAttacking = false;
        }

        private IEnumerator WaitForDeadEnd()
        {
            if (Animator is null)
            {
                Debug.LogError("Animator component is missing on this GameObject.");
                yield break;
            }
            _isDead = true;
            Animator.Play("death");
            yield return null;
            
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            var length = stateInfo.length;
            Debug.Log($"Monster {name} is dead.");
            
            yield return new WaitForSeconds(length + 2f); // Wait for the death animation to finish
            Destroy(gameObject);
        }
        
        #endregion
        
        #region Public Methods

        public void OnAnimationFinished()
        {
            StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(5f); // Delay before destruction
            Destroy(gameObject);
        }

        // private IEnumerator PatrolCoroutine()
        // {
        //     _isPatrol = true;   // 페트롤 상태로 전환
        //     patrol.GenIndex();  // 
        //     patrol.GenTime();
        //     
        //     var time = 
        //     while ()
        // }
        
        #endregion
    }
}