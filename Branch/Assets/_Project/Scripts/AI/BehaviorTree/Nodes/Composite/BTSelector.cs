using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Composite/Selector")]
    // 모든 자식 노드를 순차적으로 실행하고,
    // 첫 번째 성공 노드가 발견되면 성공 상태를 반환합니다.
    // 모든 자식 노드가 실패하면 실패 상태를 반환합니다.
    public class BTSelector : BTComposite
    {
        // public BTSelector(NodeState state, string name, string guid, Vector2 position, List<BTNode> children)
        // {
        //     this.state = state;
        //     this.name = name;
        //     this.guid = guid.ToString();
        //     this.position = position;
        //     this.children = children ?? new List<BTNode>();
        // }
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            foreach (var child in children)
            {
                var childState = child.Evaluate(monsterStats,  visited);
                if (childState == NodeState.Success)
                {
                    state = NodeState.Success;
                    return state;
                }
                if (childState == NodeState.Running)
                {
                    state = NodeState.Running;
                    return state;
                }
            }
            state = NodeState.Failure;
            return state;
        }
    }
}