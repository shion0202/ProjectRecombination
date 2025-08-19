using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    public abstract class BTCondition : BTNode
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            return CheckCondition(monsterStats) ? state = NodeState.Success : state = NodeState.Failure;
        }

        protected abstract bool CheckCondition(MonsterStats monsterStats);
    }
}