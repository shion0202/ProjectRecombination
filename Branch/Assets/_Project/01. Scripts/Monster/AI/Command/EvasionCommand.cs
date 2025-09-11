using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class EvasionCommand : AICommand
    {
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, System.Action onComplete)
        {
            if (CheckBlackboard(blackboard)) yield break;

            if (blackboard.State is MonsterState.Evasion) yield break;

            // // 회피 상태 처리
            // blackboard.NavMeshAgent.isStopped = false;
            // blackboard.NavMeshAgent.ResetPath();
            //
            // // 회피 애니메이션 재생
            // blackboard.SetAnimationState(MonsterState.Evasion);
            // blackboard.State = MonsterState.Evasion; // 몬스터의 상태를 Evasion으로 설정
            //
            // // 일정 시간 동안 회피 상태 유지 (예: 1초)
            // float evasionDuration = 1.0f; // 회피 지속 시간
            // float elapsedTime = 0f;
            //
            // while (elapsedTime < evasionDuration)
            // {
            //     elapsedTime += Time.deltaTime;
            //     yield return null;
            // }
            //
            // // 회피 상태 종료 후 Idle 상태로 전환
            // blackboard.SetAnimationState(MonsterState.Idle);
            // blackboard.State = MonsterState.Idle;
            
            // TODO: 현재 사용할 회피 애니메이션이 없음으로 직접 Transform을 이동시키는 방식으로 구현
            // 추후 애니메이션 적용 시 로직 수정
            
            // 회피 방향 설정 (좌 또는 우)
            Vector3 evasionDirection = (Random.value > 0.5f) ? blackboard.Agent.transform.right : -blackboard.Agent.transform.right;
            const float evasionDistance = 3.0f; // 회피 거리
            Vector3 targetPosition = blackboard.Agent.transform.position + evasionDirection * evasionDistance;
            targetPosition.y = blackboard.Agent.transform.position.y; // y축 고정
            
            // 목표 지점과 agent 사이에 장애물이 있는지 확인
            if (Physics.Raycast(blackboard.Agent.transform.position, evasionDirection, out RaycastHit hitInfo, evasionDistance))
            {
                // 장애물이 agent와 가까우면 반대 방향으로 회피
                if (hitInfo.distance < evasionDistance / 2)
                {
                    evasionDirection = -evasionDirection;
                }
                else
                {
                    // 장애물에서 agent 방향으로 약간 떨어진 지점으로 설정
                    targetPosition = hitInfo.point - evasionDirection * 0.5f;
                    targetPosition.y = blackboard.Agent.transform.position.y; // y축 고정
                }
            }
            
            const float evasionSpeed = 5.0f; // 회피 속도
            float distanceToTarget = Vector3.Distance(blackboard.Agent.transform.position, targetPosition);
            while (distanceToTarget > 0.1f)
            {
                blackboard.NavMeshAgent.Move(evasionDirection * (evasionSpeed * Time.deltaTime));
                distanceToTarget = Vector3.Distance(blackboard.Agent.transform.position, targetPosition);
                yield return null;
            }
            blackboard.NavMeshAgent.ResetPath();
            blackboard.NavMeshAgent.isStopped = true;
            blackboard.State = MonsterState.Idle; // 몬스터의 상태를 Idle로 설정

            onComplete?.Invoke();
        }

        private static bool CheckBlackboard(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute EvasionCommand.");
                return true;
            }
            // NavMeshAgent가 유효한지 확인
            if (blackboard.NavMeshAgent is null)
            {
                Debug.LogError("NavMeshAgent is null. Cannot execute EvasionCommand.");
                return true;
            }
            if (blackboard.Agent is null)
            {
                Debug.LogError("Agent is null. Cannot execute EvasionCommand.");
                return true;
            }

            return false;
        }
    }
}