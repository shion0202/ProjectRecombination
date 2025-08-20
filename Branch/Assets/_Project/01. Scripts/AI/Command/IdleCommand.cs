using Monster;
using UnityEngine;

namespace AI.Command
{
    public class IdleCommand : AICommand
    {
        public override void Execute(Blackboard.Blackboard blackboard)
        {
            if (blackboard == null)
            {
                Debug.LogError("Blackboard is null. Cannot execute IdleCommand.");
                return;
            }
            // Idle 상태 처리
            if (blackboard.State == MonsterState.Idle)
            {
                Debug.Log("AI is already idle.");
                return;
            }
            // NavMeshAgent를 정지시키고, 이동을 중지
            if (blackboard.NavMeshAgent != null)
            {
                blackboard.NavMeshAgent.isStopped = true;
                blackboard.NavMeshAgent.ResetPath();
                Debug.Log("AI has stopped moving.");
            }
            
            // 에이전트의 위치를 유지
            if (blackboard.Agent != null)
            {
                var agent = blackboard.Agent;
                agent.transform.position = agent.transform.position; // 위치를 그대로 유지
            }
            
            // Idle 애니메이션 재생
            // TODO: Code to play idle animation can be added here
            
            // Idle 상태로 전환
            blackboard.State = MonsterState.Idle;
            Debug.Log("AI is now idle.");
        }
    }
}