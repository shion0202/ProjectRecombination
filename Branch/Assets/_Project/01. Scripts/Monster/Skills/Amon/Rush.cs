using Monster.AI.Blackboard;
using System;
using System.Collections;
using UnityEngine;

namespace _Test.Skills
{
    /// <summary>
    /// 스킬 이름: 돌진
    /// - 캐스팅: 2초 (캐스팅 중 이동 불가 상태)
    /// - 효과: 대상을 향해 돌진, 적중 시 넉백 및 대미지
    /// - 예외처리1: 캐스팅 중 스턴, 넉백, 이동 불가 상태가 되면 스킬 취소
    /// - 예외처리2: 대상과 거리가 너무 멀 경우 스킬 취소
    /// - 예외처리3: 돌진 중 장애물에 부딪히면 돌진 취소
    /// </summary>
    
    [CreateAssetMenu(fileName = "Rush", menuName = "MonsterSkills/Amon/Rush")]
    public class Rush : SkillData
    {
        public float rushSpeed;

        public override IEnumerator Activate(Blackboard data)
        {
            Debug.Log("돌진!");
            Vector3 directionToTarget = (data.Target.transform.position - data.Agent.transform.position).normalized;
            data.Agent.transform.LookAt(data.Target.transform);
            data.AnimatorParameterSetter.Animator.SetTrigger("Rush");
            data.NavMeshAgent.enabled = false;
            data.AgentRigidbody.isKinematic = false;
            data.AgentRigidbody.velocity = directionToTarget * rushSpeed; // 돌진 속도 설정
            // TODO: 돌진을 시도하고 일정 시간 충돌을 감지하고 멈추는 로직 구현
            
            float rushDuration = 2.0f; // 돌진 지속 시간
            float elapsed = 0f;
        
            while (elapsed < rushDuration)
            {
                // 장애물 충돌 체크
                if (Physics.Raycast(data.Agent.transform.position, directionToTarget, out RaycastHit hit, 1f))
                {
                    Debug.Log("장애물에 부딪혀 돌진 취소");
                    
                    break; // 장애물에 부딪히면 취소
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            Debug.Log("돌진 완료");
            data.AgentRigidbody.isKinematic = false;
            data.AnimatorParameterSetter.Animator.SetTrigger("RushEnd");
            data.AgentRigidbody.velocity = Vector3.zero;
            data.NavMeshAgent.enabled = true;
        }

        public override IEnumerator Casting(Blackboard data)
        {
            Debug.Log("돌진 시전 준비!");
            data.AgentRigidbody.velocity = Vector3.zero;
            data.AgentRigidbody.isKinematic = true;
            data.AnimatorParameterSetter.Animator.SetTrigger("RushReady");
            return base.Casting(data);
        }
    }
}

// TODO: 리지드바디를 이용해 돌진을 구현하였으나 물체와 충돌 후 멈추는 것이 자연스럽지 않음 (오히려 관성으로 더 멀리 감)
// 제자리에 멈출 수 있는 방법이 무었이 있을까?