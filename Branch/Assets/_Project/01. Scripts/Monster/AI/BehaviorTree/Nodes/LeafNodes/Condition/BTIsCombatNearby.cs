using Monster.AI.Blackboard;
using Managers;
using UnityEngine;

namespace Monster.AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTIsCombatNearby", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Condition/BTIsCombatNearby")]
    public class BTIsCombatNearby : BTCondition
    {
        protected override bool CheckCondition(NodeContext context)
        {
            Blackboard.Blackboard blackboard = context.Blackboard;
            
            if (blackboard.TryGet(new BBKey<float>("detectionRange"), out float detectionRange))
            {
                // 근처에서 싸움이 벌어졌는지 메니저를 통해 확인
                // 현재 에이전트의 위치와 감지 범위를 사용하여 근처에 몬스터 전투가 있는지 확인
                return MonsterManager.Instance.IsNearMonsterBattle(blackboard.Agent, detectionRange);
            }

            return false;
        }
    }
}