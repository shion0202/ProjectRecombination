using System.Collections.Generic;
using AI.Command;
using Monster;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTActionIdle", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Actions/BTActionIdle")]
    public class BTActionIdle : BTAction
    {
        public override NodeState Evaluate(NodeContext context, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            // // Idle 행동은 단순히 대기 상태를 유지하는 행동이다.
            // // 이 행동은 일반적으로 AI가 아무것도 하지 않고 대기할 때 사용
            // var blackboard = context.Blackboard;
            //
            // blackboard.NavMeshAgent.Move(Vector3.zero);                   // NavMeshAgent를 사용하여 이동을 멈춘다.
            // // monsterMonoBehaviour.SetAnimationState(MonsterState.Idle);   // 애니메이션 상태를 Idle로 설정
            //
            // // 대기 애니메이션을 액션 스크립트에서 재생해야 할까?
            //
            // // Idle 행동은 항상 성공 상태를 반환한다.
            // blackboard.State = MonsterState.Idle; // 몬스터의 상태를 Idle로 설정
            
            // Idle 명령을 행동 대기열에 추가
            context.Enqueue(new IdleCommand());
            
            state = NodeState.Success;
            return state;
        }
    }
}