using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    // 노드가 성공 상태가 될 때까지 실행
    [CreateAssetMenu(menuName = "BehaviorTree/Decorator/UntilSuccess")]
    public class BTUntilSuccess : BTDecorator
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            var nodeState = child.Evaluate(monsterStats, visited);

            if (nodeState == NodeState.Success)
                return state = nodeState;

            return state = NodeState.Running;
        }
    }
}