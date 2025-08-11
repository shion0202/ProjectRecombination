using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Action/Attack2Skill")]
    public class BTActionAttack2Skill : BTAction
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            Debug.Log("Monster is Using Skill 2: " + monsterStats.name);
            monsterStats.State = MonsterState.Attack;
            return state = NodeState.Success;
        }
    }
}