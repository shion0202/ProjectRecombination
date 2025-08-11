using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsDeath")]
    public class BTIsDeath : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // 몬스터의 생명력이 0 이하인지 확인
            return monsterStats.Health <= 0f;
        }
    }
}