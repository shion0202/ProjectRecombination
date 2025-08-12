using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsInAttack1")]
    public class BTIsInAttack1 : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // 몬스터가 스킬 1을 사용할 수 있는 상태인지 확인
            
            if (monsterStats is null)
            {
                Debug.LogError("MonsterStats is null in BTIsInAttack1.");
                return false;
            }
            if (monsterStats.Attack1Range <= 0f)
            {
                Debug.LogError("Attack1Range is not set in MonsterStats.");
                return false;
            }
            if (monsterStats.Target is null)
            {
                Debug.LogError("Target is null in BTIsInAttack1.");
                return false;
            }
            // 몬스터의 타겟과의 거리가 공격 범위 이내인지 확인
            
            // var distanceToTarget = Vector3.Distance(monsterStats.Agent is not null ? monsterStats.Agent.transform.position : monsterStats.transform.position, monsterStats.Target.transform.position);
            // TODO: StackOverflowException: The requested operation caused a stack overflow.
            if (monsterStats.TargetDistance <= monsterStats.Attack1Range)
                return true;
            return false;
        }
    }
}