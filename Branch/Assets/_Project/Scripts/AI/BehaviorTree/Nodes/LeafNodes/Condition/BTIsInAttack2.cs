using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsInAttack2")]
    public class BTIsInAttack2 : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // 몬스터가 스킬 1을 사용할 수 있는 상태인지 확인
            
            if (monsterStats == null)
            {
                Debug.LogError("MonsterStats is null in BTIsInAttack2.");
                return false;
            }
            if (monsterStats.Attack2Range <= 0f)
            {
                Debug.LogError("Attack2Range is not set in MonsterStats.");
                return false;
            }
            if (monsterStats.Target == null)
            {
                Debug.LogError("Target is null in BTIsInAttack2.");
                return false;
            }
            // 몬스터의 타겟과의 거리가 공격 범위 이내인지 확인
            if (monsterStats.TargetDistance <= monsterStats.Attack2Range)
                return true;
            return false;
        }
    }
}