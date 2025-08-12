using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsRandom")]
    public class BTIsRandom : BTCondition
    {
        public float threshold = 0.2f; // 20% 확률
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // 몬스터가 랜덤 행동을 할 확률을 결정
            // 예를 들어, 20% 확률로 true를 반환
            var randomValue = Random.value; // 0.0f ~ 1.0f 사이의 랜덤 값 생성

            return randomValue < threshold;
        }
    }
}