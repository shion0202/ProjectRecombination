using Monster;
using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class IdleCommand : AICommand
    {
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            if (blackboard == null)
            {
                Debug.LogError("Blackboard is null. Cannot execute IdleCommand.");
                yield break;
            }
            // Idle 상태 처리
            if (blackboard.State == MonsterState.Idle)
            {
                Debug.Log("AI is already idle.");
                yield break;
            }
            // NavMeshAgent를 정지시키고, 이동을 중지
            if (blackboard.NavMeshAgent != null)
            {
                blackboard.NavMeshAgent.isStopped = true;
                blackboard.NavMeshAgent.ResetPath();
                Debug.Log("AI has stopped moving.");
            }
            
            // Idle 상태로 전환
            blackboard.State = MonsterState.Idle;
            // Debug.Log("AI is now idle.");
            
            // 명령어 완료 콜백 호출
            onComplete?.Invoke();
        }
    }
}