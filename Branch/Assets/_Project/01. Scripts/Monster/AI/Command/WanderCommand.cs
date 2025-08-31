using Monster.AI.Blackboard;
using System.Collections;
using System;
using UnityEngine;

namespace Monster.AI.Command
{
    public class WanderCommand : AICommand
    {
        // private static bool _animationRunning;
        private static bool CheckBlackboard(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute Wander Command.");
                return false;
            }
            
            // Wander 정보가 유효한지 확인
            if (blackboard.WanderInfo is null)
            {
                Debug.LogWarning("WanderInfo is not valid for WanderCommand.");
                return false;
            }
                
            if (blackboard.WanderInfo.wanderAreaRadius <= 0f)
            {
                Debug.LogWarning("Wander area radius is not set or invalid.");
                return false;
            }
                
            // NavMeshAgent가 유효한지 확인
            if (blackboard.NavMeshAgent is null)
            {
                Debug.LogError("NavMeshAgent is null. Cannot execute Wander Command.");
                return false;
            }

            return true;
        }
        // private static bool CheckAnimator(Blackboard.Blackboard blackboard, string animationName)
        // {
        //     if (blackboard.Animator is null)
        //     {
        //         Debug.LogWarning("Animator is null. Cannot play Wander animation.");
        //         return false;
        //     }
        //     if (!blackboard.Animator.HasState(0, Animator.StringToHash(animationName)))
        //     {
        //         Debug.LogWarning("Animator does not have a 'Walk' state. Please ensure the animation is set up correctly.");
        //         return false;
        //     }
        //     // 현재 재생 중인 에니메이션이 animationName일 경우
        //     if (blackboard.Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        //     {
        //         Debug.Log("Wander animation is already playing.");
        //         return false;
        //     }
        //     return true;
        // }
        
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            if (CheckBlackboard(blackboard)) yield break;
            
            // Wander 진행
            if (blackboard.State is MonsterState.Wander)
            {
                // 현재 Wander 시간이 초과되었는지 확인
                if (Time.time - blackboard.WanderInfo.StartWanderTime >= blackboard.WanderInfo.CurrentWanderTime)
                {
                    // blackboard.WanderInfo.IsWandering = false;
                    // Debug.Log("Wander time exceeded. Stopping wandering.");
                    blackboard.State = MonsterState.Idle;
                }
                else
                {
                    // 현재 Wander 지점으로 이동 중
                    if (blackboard.NavMeshAgent.remainingDistance <= blackboard.NavMeshAgent.stoppingDistance)
                    {
                        // Debug.Log("Reached current wander point. Continuing to wander.");
                        // blackboard.WanderInfo.IsWandering = false; // 현재 Wander를 종료하고 새로운 Wander를 시작
                        blackboard.WanderInfo.CurrentWanderPoint = blackboard.WanderInfo.GetRandomWanderPoint();
                        blackboard.NavMeshAgent.destination = blackboard.WanderInfo.CurrentWanderPoint;
                        blackboard.NavMeshAgent.isStopped = false; // 이동을 계속
                    }
                }
            }
            else
            {
                // Wander 상태로 전환
                blackboard.State = MonsterState.Wander;
                Debug.Log("AI is now wander.");
                
                blackboard.WanderInfo.StartWanderTime = Time.time;
                blackboard.WanderInfo.CurrentWanderTime = blackboard.WanderInfo.GetRandomWanderTime();
                blackboard.WanderInfo.CurrentWanderPoint = blackboard.WanderInfo.GetRandomWanderPoint();
                blackboard.NavMeshAgent.destination = blackboard.WanderInfo.CurrentWanderPoint;
                blackboard.NavMeshAgent.isStopped = false; // 이동을 시작
                Debug.Log("AI is now wandering to a new point.");
                
                // NavMesh Speed 설정
                blackboard.NavMeshAgent.speed = blackboard.TryGet(new BBKey<float>("walkSpeed"), out float speed) ? speed : 0;
                
            }
            
            // Wander 애니메이션 재생
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