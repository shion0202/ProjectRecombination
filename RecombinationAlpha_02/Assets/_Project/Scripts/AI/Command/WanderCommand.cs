using System.Collections;
using Monster;
using UnityEngine;

namespace AI.Command
{
    public class WanderCommand : AICommand
    {
        public override void Execute(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute Wander Command.");
                return;
            }
            // Wander 상태 처리
            {
                // Wander 정보가 유효한지 확인
                if (blackboard.WanderInfo == null)
                {
                    Debug.LogWarning("WanderInfo is not valid for WanderCommand.");
                    return;
                }
                
                if (blackboard.WanderInfo.wanderAreaRadius <= 0f)
                {
                    Debug.LogWarning("Wander area radius is not set or invalid.");
                    return;
                }
                
                // NavMeshAgent가 유효한지 확인
                if (blackboard.NavMeshAgent is null)
                {
                    Debug.LogError("NavMeshAgent is null. Cannot execute Wander Command.");
                    return;
                }
                
                // Wander 상태로 전환
                blackboard.State = MonsterState.Wander;
                Debug.Log("AI is now wander.");
                
                // Wander 진행
                if (blackboard.WanderInfo.IsWandering)
                {
                    // 현재 Wander 시간이 초과되었는지 확인
                    if (Time.time - blackboard.WanderInfo.StartWanderTime >= blackboard.WanderInfo.CurrentWanderTime)
                    {
                        blackboard.WanderInfo.IsWandering = false;
                        Debug.Log("Wander time exceeded. Stopping wandering.");
                    }
                    else
                    {
                        // 현재 Wander 지점으로 이동 중
                        if (blackboard.NavMeshAgent.remainingDistance <= blackboard.NavMeshAgent.stoppingDistance)
                        {
                            Debug.Log("Reached current wander point. Continuing to wander.");
                            blackboard.WanderInfo.IsWandering = false; // 현재 Wander를 종료하고 새로운 Wander를 시작
                        }
                        // else
                        // {
                        //     Debug.Log("Continuing to wander to the current point.");
                        //     return; // 현재 Wander 지점으로 이동 중이므로 추가 작업을 하지 않음
                        // }
                    }
                }
                else
                {
                    blackboard.WanderInfo.IsWandering = true;
                    blackboard.WanderInfo.StartWanderTime = Time.time;
                    blackboard.WanderInfo.CurrentWanderTime = blackboard.WanderInfo.GetRandomWanderTime();
                    blackboard.WanderInfo.CurrentWanderPoint = blackboard.WanderInfo.GetRandomWanderPoint();
                    blackboard.NavMeshAgent.destination = blackboard.WanderInfo.CurrentWanderPoint;
                    blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
                    Debug.Log("AI is now wandering to a new point.");
                }
            }
            
            // Wander 애니메이션 재생
            // TODO: Code to play Patrol animation can be added here
        }
        
        
    }
}