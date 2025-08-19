using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Action/Idle")]
    public class BTActionIdle : BTAction
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            // Idle 행동은 단순히 대기 상태를 유지하는 행동이다.
            // 이 행동은 일반적으로 AI가 아무것도 하지 않고 대기할 때 사용
            
            monsterStats.NavMeshAgent.Move(Vector3.zero);                   // NavMeshAgent를 사용하여 이동을 멈춘다.
            // monsterMonoBehaviour.SetAnimationState(MonsterState.Idle);   // 애니메이션 상태를 Idle로 설정
            
            // 대기 애니메이션을 액션 스크립트에서 재생해야 할까?
            
            // Idle 행동은 항상 성공 상태를 반환한다.
            monsterStats.State = MonsterState.Idle; // 몬스터의 상태를 Idle로 설정
            state = NodeState.Success;
            return state;
        }
    }
}