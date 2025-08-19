using Monster;
using UnityEngine;

namespace AI.Command
{
    public class ChaseCommand : AICommand
    {
        public override void Execute(Blackboard.Blackboard blackboard)
        {
            if (blackboard == null)
            {
                Debug.LogError("Blackboard is null. Cannot execute ChaseCommand.");
                return;
            }
            
            // Chase 상태 처리
            {
                // 타겟의 위치가 유효한지 확인
                if (blackboard.Target == null || !blackboard.Target.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("Target is not valid for ChaseCommand.");
                    return;
                }
                
                // NavMeshAgent가 유효한지 확인
                if (blackboard.NavMeshAgent == null)
                {
                    Debug.LogWarning("NavMeshAgent is null. Cannot execute ChaseCommand.");
                    return;
                }
                
                // Chase 진행
                blackboard.NavMeshAgent.destination = blackboard.Target.gameObject.transform.position;
                blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
                Debug.Log("AI is now chasing the target.");
            }
            
            // Chase 애니메이션 재생
            // TODO: Code to play chase animation can be added here
            
            // Chase 상태로 전환
            blackboard.State = MonsterState.Chase;
            Debug.Log("AI is now Chase.");
        }
    }
}