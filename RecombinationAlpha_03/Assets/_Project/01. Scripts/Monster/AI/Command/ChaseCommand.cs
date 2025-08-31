using Monster.AI.Blackboard;
using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class ChaseCommand : AICommand
    {
        private static bool CheckBlackboard(Blackboard.Blackboard blackboard)
        {
            if (blackboard?.NavMeshAgent is null)
            {
                Debug.LogError("NavMeshAgent is null. Cannot execute ChaseCommand.");
                return false;
            }
            
            // 타겟의 위치가 유효한지 확인
            if (blackboard.Target is null || !blackboard.Target.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Target is not valid for ChaseCommand.");
                return false;
            }
            return true;
        }
        
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            if (!CheckBlackboard(blackboard)) yield break;
            
            // Chase 상태 처리
            if (blackboard.State is MonsterState.Chase)
            {
                // 이미 Chase 중인 경우, 타겟과 충분히 가까운지 확인
                float distance = Vector3.Distance(blackboard.Agent.transform.position, blackboard.Target.transform.position);
                float stopDistance = blackboard.TryGet(new BBKey<float>("minDetectionRange"), out float value) ? value : 0f;
                if (distance <= stopDistance)
                {
                    // 타겟과 충분히 가까운 경우, Chase 상태를 종료하고 Idle 상태로 전환
                    // _isChasing = false;
                    // blackboard.State = MonsterState.Idle;
                    blackboard.NavMeshAgent.isStopped = true; // 이동을 멈춤
                    blackboard.NavMeshAgent.ResetPath(); // 경로를 초기화
                }
                else
                {
                    // 타겟과 충분히 멀리 있는 경우, 계속 Chase 상태 유지
                    blackboard.NavMeshAgent.SetDestination(blackboard.Target.gameObject.transform.position); // 타겟 위치로 이동 경로 설정
                    // blackboard.NavMeshAgent.isStopped = false; // 이동을 계속
                }
            }
            else
            {
                // Chase 상태로 전환
                blackboard.State = MonsterState.Chase;
                // Debug.Log("AI is now Chase.");
                blackboard.NavMeshAgent.speed = blackboard.TryGet(new BBKey<float>("runSpeed"), out float speed) ? speed : 0;   // NavMesh Speed 설정
                blackboard.NavMeshAgent.SetDestination(blackboard.Target.gameObject.transform.position); // 타겟 위치로 이동 경로 설정
                blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
            }
            
            // 이동 애니메이션 재생
            if (CheckAnimator(blackboard, "Run"))
            {
                blackboard.Animator.SetTrigger("Run");
            }
            
            // 명령어 완료 콜백 호출
            onComplete?.Invoke();
        }
    }
}