using System.Collections.Generic;
using AI.Command;
using Monster;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTActionChase", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Actions/BTActionChase")]
    public class BTActionChase : BTAction
    {
        public override NodeState Evaluate(NodeContext context, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            // 여기서는 단순히 상태를 반환하고 종료함
            // Debug.Log("Moving to target: " + context.target.name);
            var blackboard = context.Blackboard;
           
            // 몬스터가 타겟과 충분히 가까운지 확인하여, 너무 가까우면 이동을 멈추고 성공 상태 반환
            float stopDistance = blackboard.NavMeshAgent.stoppingDistance;
            float distance = Vector3.Distance(blackboard.Agent.transform.position, blackboard.Target.transform.position);
            if (distance <= stopDistance + 1f)
            {
                Debug.Log("Monster is close enough to the target: " + blackboard.name);
                // Idle 명령을 행동 대기열에 추가
                context.Enqueue(new IdleCommand());
                state = NodeState.Failure;
                return state;
            }
            
            // // 몬스터 자신의 NavMeshAgent를 받아와서 이동 경로로 설정
            // blackboard.NavMeshAgent.SetDestination(blackboard.Target.transform.position);
            //
            // blackboard.State = MonsterState.Chase;
            
            // Chase 명령을 행동 대기열에 추가
            context.Enqueue(new ChaseCommand());
            state = NodeState.Running;
            return state;
        }
    }
}