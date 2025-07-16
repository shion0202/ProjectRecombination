using System.Collections;
using UnityEngine;

namespace Jaeho.Monster
{
    public class MonsterNormal : MonsterBase
    {
        #region Default Methods

        protected override void Idle()
        {
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for idle state.");
                return;
            }
            
            SetIdleAnima(Random.Range(1, 4));
            
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
            
            // Set Run animation
            SetRunAnima();
            
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
            
            // Attack logic
            Debug.Log($"Attacking target: {Target.name} with damage: {Damage}");

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
            Animator.SetBool("IsDead", true);
            // 모든 코루틴 실행 정지
        }

        #endregion
        
        #region Animation

        private void InitAnima()
        {
            Animator.SetBool("IsIdle", false);
            Animator.SetBool("IsWalk", false);
            Animator.SetBool("IsRun", false);
            Animator.SetBool("IsAttack", false);
            
        }
        
        private void SetIdleAnima(int nValue = 0)
        {
            if (Animator is null)
            {
                Debug.LogError("Animator component is missing on this GameObject.");
                return;
            }
            
            {
                Animator.SetBool("IsIdle", 0 < nValue);
                Animator.SetBool("IsWalk", false);
                Animator.SetBool("IsRun", false);
                Animator.SetBool("IsAttack", false);
            }
            Animator.SetInteger("IdleKey", nValue);
        }

        private void SetRunAnima()
        {
            if (Animator is null)
            {
                Debug.LogError("Animator component is missing on this GameObject.");
                return;
            }

            {
                Animator.SetBool("IsRun", true);
                Animator.SetBool("IsIdle", false);
                Animator.SetBool("IsWalk", false);
                Animator.SetBool("IsAttack", false);
            }
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
        
        #endregion
    }
}