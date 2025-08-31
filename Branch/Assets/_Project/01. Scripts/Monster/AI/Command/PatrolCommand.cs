using Monster.AI.Blackboard;
using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class PatrolCommand : AICommand
    {
        // private static bool _animationRunning;
        private static bool CheckBlackboard(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute Wander Command.");
                return false;
            }
            
            // Patrol 정보가 유효한지 확인
            if (blackboard.PatrolInfo == null)
            {
                Debug.LogWarning("PatrolInfo is not valid for PatrolCommand.");
                return false;
            }
                
            if (blackboard.PatrolInfo.wayPoints == null || blackboard.PatrolInfo.wayPoints.Length == 0)
            {
                Debug.LogWarning("Patrol waypoints are not set or invalid.");
                return false;
            }
                
            // NavMeshAgent가 유효한지 확인
            if (blackboard.NavMeshAgent is null)
            {
                Debug.LogError("NavMeshAgent is null. Cannot execute Patrol Command.");
                return false;
            }

            return true;
        }
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            if (!CheckBlackboard(blackboard)) yield break;
            
            // Patrol 상태 처리
            if (blackboard.State == MonsterState.Patrol)
            {
                // 현재 Patrol 시간이 초과되었는지 확인
                if (Time.time - blackboard.PatrolInfo.StartPatrolTime >= blackboard.PatrolInfo.CurrentPatrolTime)
                {
                    // blackboard.PatrolInfo.IsPatrolling = false;
                    // Debug.Log("Patrol time exceeded. Stopping patrolling.");
                    blackboard.State = MonsterState.Idle;
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
                }
            }
            else
            {
                // blackboard.PatrolInfo.IsPatrolling = true;
                blackboard.State = MonsterState.Patrol;
                blackboard.PatrolInfo.StartPatrolTime = Time.time;
                blackboard.PatrolInfo.CurrentPatrolTime = blackboard.PatrolInfo.GetRandomPatrolTime();
                blackboard.PatrolInfo.CurrentWayPointIndex = blackboard.PatrolInfo.GetNextWayPointIndex();
                blackboard.NavMeshAgent.destination = blackboard.PatrolInfo.GetCurrentWayPoint();
                blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
                Debug.Log("AI is now wandering to a new point.");
                
                // NavMesh Speed 설정
                blackboard.NavMeshAgent.speed = blackboard.TryGet(new BBKey<float>("walkSpeed"), out float speed) ? speed : 0;
                // blackboard.Animator.SetTrigger("Run");
            }
            
            // Patrol 애니메이션 재생
            if (CheckAnimator(blackboard, "Run"))
            {
                // _animationRunning = true;
                blackboard.Animator.SetTrigger("Run");
            }
            
            // 명령어 완료 콜백 호출
            onComplete?.Invoke();
        }
    }
}