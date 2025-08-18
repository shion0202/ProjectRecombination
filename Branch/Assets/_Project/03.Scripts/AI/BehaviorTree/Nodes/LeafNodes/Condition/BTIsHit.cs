using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsHit")]
    public class BTIsHit : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // 몬스터가 피격당했는지 확인
            return monsterStats.Health < monsterStats.MaxHealth;
        }
    }
}