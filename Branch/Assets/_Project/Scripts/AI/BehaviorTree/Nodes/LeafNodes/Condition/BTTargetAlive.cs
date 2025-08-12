using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsTargetAlive")]
    public class BTIsTargetAlive : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            var target = monsterStats.Target;
            return target is not null;
            
            // TODO: 실제 사용 시 target이 살아있는지 확인하는 로직 추가 필요
            // 게임 메니저를 통해 target이 살아있는지 확인하는 로직을 추가할 수 있습니다.
        }
    }
}