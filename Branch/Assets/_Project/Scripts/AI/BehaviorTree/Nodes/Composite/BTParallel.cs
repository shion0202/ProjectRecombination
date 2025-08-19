using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    // 여러 자식을 동시에 실행
    // 성공 여부는 별도로 지정
    [CreateAssetMenu(menuName = "BehaviorTree/Composite/Parallel")]
    public class BTParallel : BTComposite
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            // 모든 자식 노드를 실행만 함
            foreach (var child in children)
            {
                child.Evaluate(monsterStats,  visited);
            }
            
            // 실행 즉시 성공 반환
            return state = NodeState.Success;
        }
    }
}