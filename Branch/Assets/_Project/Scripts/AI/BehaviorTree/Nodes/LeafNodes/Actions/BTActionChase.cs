using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Action/Chase")]
    public class BTActionChase : BTAction
    {
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            // 여기서는 단순히 상태를 반환하고 종료함
            // Debug.Log("Moving to target: " + blackboard.target.name);
            
           
            // 몬스터가 타겟과 충분히 가까운지 확인하여, 너무 가까우면 이동을 멈추고 성공 상태 반환
            float stopDistance = monsterStats.NavMeshAgent.stoppingDistance;
            float distance = Vector3.Distance(monsterStats.Agent.transform.position, monsterStats.Target.transform.position);
            if (distance <= stopDistance + 0.1f)
            {
                Debug.Log("Monster is close enough to the target: " + monsterStats.name);
                monsterStats.NavMeshAgent.isStopped = true;
                monsterStats.State = MonsterState.Chase;
                state = NodeState.Success;
                return state;
            }
            
            // 몬스터 자신의 NavMeshAgent를 받아와서 이동 경로로 설정
            monsterStats.NavMeshAgent.SetDestination(monsterStats.Target.transform.position);

            monsterStats.State = MonsterState.Chase;
            
            state = NodeState.Running;
            return state;
        }
    }
}