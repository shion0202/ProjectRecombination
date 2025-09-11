using Managers;
using Monster;
using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class DeathCommand : AICommand
    {
        private Collider _collider;
        private Rigidbody _rigidbody;
        
        private bool CheckBlackboard(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute DeathCommand.");
                return false;
            }
            // NavMeshAgent가 유효한지 확인
            if (blackboard.NavMeshAgent is null)
            {
                Debug.LogError("NavMeshAgent is null. Cannot execute DeathCommand.");
                return false;
            }
            if (blackboard.Agent is null)
            {
                Debug.LogError("Agent is null. Cannot execute DeathCommand.");
                return false;
            }

            _collider = blackboard.AgentCollider;
            if (_collider is null)
            {
                Debug.LogError("Collider is null. Cannot execute DeathCommand.");
                return false;
            }
            
            _rigidbody = blackboard.AgentRigidbody;
            if (_rigidbody is null)
            {
                Debug.LogError("Rigidbody is null. Cannot execute DeathCommand.");
                return false;
            }
            
            return true;
        }
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            if (!CheckBlackboard(blackboard)) yield break;
            
            if (blackboard.State is MonsterState.Death) yield break;
            
            // 죽음 상태 처리
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
            blackboard.NavMeshAgent.isStopped = true;
            blackboard.NavMeshAgent.ResetPath();
            
        
            // Animator가 유효한지 확인
            if (CheckAnimator(blackboard, "Death"))
            {
                // 재생 중인 모든 애니메이션 초기화
                blackboard.Animator.Rebind();
                blackboard.Animator.Update(0f);

                // Death 애니메이션 재생
                blackboard.Animator.SetTrigger("Death");
                yield return null;

                AnimatorStateInfo animaState = blackboard.Animator.GetCurrentAnimatorStateInfo(0);
                float animationLength = animaState.length;
                yield return new WaitForSeconds(animationLength); // 애니메이션 재생 시간 대기 (예: 2초)
            }
            
            blackboard.DeathEffect.SetActive(true);
            yield return new WaitForSeconds(2.0f);
            blackboard.DeathEffect.SetActive(false);
            
            blackboard.State = MonsterState.Death;
            yield return null;
            
            // 명령어 완료 콜백 호출
            onComplete?.Invoke();
        }
    }
}