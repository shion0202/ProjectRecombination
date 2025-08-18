using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    // 자식 노드의 실행 결과가 실패가 발생할 때까지 반복 실행
    [CreateAssetMenu(menuName = "BehaviorTree/Decorator/UntilFailure")]
    public class BTUntilFailure : BTDecorator
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            var nodeState = child.Evaluate(monsterStats, visited);

            if (nodeState == NodeState.Failure)
                return state = nodeState;

            return state = NodeState.Running;
        }
    }
}