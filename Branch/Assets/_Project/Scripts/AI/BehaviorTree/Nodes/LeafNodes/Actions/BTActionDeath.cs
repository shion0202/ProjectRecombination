using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Action/Death")]
    public class BTActionDeath : BTAction
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            Debug.Log("Monster is dead: " + monsterStats.name);
            // 몬스터가 사망했을 때의 처리 로직
            // 예를 들어, NavMeshAgent를 정지시키고 상태를 Dead로 설정
            monsterStats.NavMeshAgent.isStopped = true; // NavMeshAgent를 정지시
            monsterStats.NavMeshAgent.ResetPath(); // 현재 경로를 초기화
            // 몬스터의 상태를 Dead로 설정
            monsterStats.State = MonsterState.Dead;
            // StartCoroutine(Coroutine());
            return state = NodeState.Success;
        }

        // private IEnumerator Coroutine()
        // {
        //     yield return new WaitForSeconds(0.5f);
        //     // 여기에 사망 후 처리 로직을 추가할 수 있습니다.
        //     // 예를 들어, 몬스터를 제거하거나 애니메이션을 재생
        //     Debug.Log("Monster has been removed or death animation played.");
        //     Debug.Log("Monster has Destroyed");
        //     Destroy(MonsterStats.Agent);
        // }
    }
}