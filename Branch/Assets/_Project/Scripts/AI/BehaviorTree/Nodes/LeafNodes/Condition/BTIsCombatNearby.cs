using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsCombatNearby")]
    public class BTIsCombatNearby : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // 근처에서 싸움이 벌어졌는지 메니저를 통해 확인
            return MonsterManager.instance.IsNearMonsterBattle(monsterStats.Agent, monsterStats.DetectionRange);
        }
    }
}