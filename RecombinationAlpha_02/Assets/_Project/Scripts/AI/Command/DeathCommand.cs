using Monster;
using UnityEngine;

namespace AI.Command
{
    public class DeathCommand : AICommand
    {
        public override void Execute(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute DeathCommand.");
                return;
            }
            // Death 상태 처리
            if (blackboard.State == MonsterState.Death)
            {
                Debug.Log("AI is already death.");
                return;
            }
            // NavMeshAgent를 정지시키고, 이동을 중지
            if (blackboard.NavMeshAgent is not null)
            {
                blackboard.NavMeshAgent.isStopped = true;
                blackboard.NavMeshAgent.ResetPath();
                Debug.Log("AI has stopped moving.");
            }
            
            // 에이전트의 위치를 유지
            if (blackboard.Agent is not null)
            {
                var agent = blackboard.Agent;
                agent.transform.position = agent.transform.position; // 위치를 그대로 유지
            }
            
            // Death 애니메이션 재생
            // TODO: Code to play death animation can be added here
            
            // Death 상태로 전환
            blackboard.State = MonsterState.Death;
            Debug.Log("AI is now death.");
        }
    }
}