using Monster;
using UnityEngine;

namespace AI.Command
{
    public class PatrolCommand : AICommand
    {
        public override void Execute(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute Wander Command.");
                return;
            }
            // Patrol 상태 처리
            {
                // Patrol 정보가 유효한지 확인
                if (blackboard.PatrolInfo == null)
                {
                    Debug.LogWarning("PatrolInfo is not valid for PatrolCommand.");
                    return;
                }
                
                if (blackboard.PatrolInfo.wayPoints == null || blackboard.PatrolInfo.wayPoints.Length == 0)
                {
                    Debug.LogWarning("Patrol waypoints are not set or invalid.");
                    return;
                }
                
                // NavMeshAgent가 유효한지 확인
                if (blackboard.NavMeshAgent is null)
                {
                    Debug.LogError("NavMeshAgent is null. Cannot execute Patrol Command.");
                    return;
                }
                
                // Patrol 상태로 전환
                blackboard.State = MonsterState.Patrol;
                Debug.Log("AI is now patrolling.");
                
                // Patrol 진행
                if (blackboard.PatrolInfo.IsPatrolling)
                {
                    // 현재 Patrol 시간이 초과되었는지 확인
                    if (Time.time - blackboard.PatrolInfo.StartPatrolTime >= blackboard.PatrolInfo.CurrentPatrolTime)
                    {
                        blackboard.PatrolInfo.IsPatrolling = false;
                        Debug.Log("Patrol time exceeded. Stopping patrolling.");
                    }
                    else
                    {
                        // 현재 Patrol 지점으로 이동 중
                        if (blackboard.NavMeshAgent.remainingDistance <= blackboard.NavMeshAgent.stoppingDistance)
                        {
                            // Debug.Log("Reached current wander point. Continuing to wander.");
                            // blackboard.PatrolInfo.IsPatrolling = false; // 현재 Patrol를 종료하고 새로운 Patrol를 시작
                            
                            // 다음 Patrol 지점으로 이동
                            blackboard.PatrolInfo.CurrentWayPointIndex = blackboard.PatrolInfo.GetNextWayPointIndex();
                            blackboard.NavMeshAgent.destination = blackboard.PatrolInfo.GetCurrentWayPoint();
                            blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
                        }
                        // else
                        // {
                        //     Debug.Log("Continuing to wander to the current point.");
                        //     return; // 현재 Patrol 지점으로 이동 중이므로 추가 작업을 하지 않음
                        // }
                    }
                }
                else
                {
                    blackboard.PatrolInfo.IsPatrolling = true;
                    blackboard.PatrolInfo.StartPatrolTime = Time.time;
                    blackboard.PatrolInfo.CurrentPatrolTime = blackboard.PatrolInfo.GetRandomPatrolTime();
                    blackboard.PatrolInfo.CurrentWayPointIndex = blackboard.PatrolInfo.GetNextWayPointIndex();
                    blackboard.NavMeshAgent.destination = blackboard.PatrolInfo.GetCurrentWayPoint();
                    blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
                    Debug.Log("AI is now wandering to a new point.");
                }
            }
            
            // Patrol 애니메이션 재생
            // TODO: Code to play Patrol animation can be added here
        }
    }
}