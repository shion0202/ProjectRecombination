using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    // 자식 노드의 실행 결과를 반대로 뒤집는다.
    [CreateAssetMenu(menuName = "BehaviorTree/Decorator/Inverter")]
    public class BTInverter : BTDecorator
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            var nodeState = child.Evaluate(monsterStats, visited);
            
            if (nodeState == NodeState.Failure)
                state = NodeState.Success;
            else if (nodeState == NodeState.Success)
                state = NodeState.Failure;
            else
                state = NodeState.Running;
            
            return state;
        }
    }
}