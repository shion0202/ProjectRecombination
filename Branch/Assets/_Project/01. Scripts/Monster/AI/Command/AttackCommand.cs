using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class AttackCommand : AICommand
    {
        // public static Coroutine coroutine;
        // private static bool _isAttacking;
        private RowData _skillRowData;

        public AttackCommand(RowData skillRowData)
        {
            _skillRowData = skillRowData;
        }
        
        private static bool CheckBlackboard(Blackboard.Blackboard blackboard)
        {
            if (blackboard is null)
            {
                Debug.LogError("Blackboard is null. Cannot execute AttackCommand.");
                return false;
            }
            // NavMeshAgent가 유효한지 확인
            if (blackboard.NavMeshAgent is null)
            {
                Debug.LogError("NavMeshAgent is null. Cannot execute AttackCommand.");
                return false;
            }
            if (blackboard.Agent is null)
            {
                Debug.LogError("Agent is null. Cannot execute AttackCommand.");
                return false;
            }
            if (blackboard.Target is null)
            {
                Debug.LogError("Target is null. Cannot execute AttackCommand.");
                return false;
            }
            
            return true;
        }
        
        public override IEnumerator Execute(Blackboard.Blackboard blackboard, Action onComplete)
        {
            if (!CheckBlackboard(blackboard)) yield break;
            if (blackboard.State == MonsterState.Attack || _skillRowData == null) yield break;
            
            // _isAttacking = true;
            blackboard.State = MonsterState.Attack;
            
            int skillID = (int)_skillRowData.Stats[EStatType.ID].value;
            float coolTime = _skillRowData.Stats[EStatType.CooldownTime].value;
            
            // Agent 움직임 정지
            blackboard.NavMeshAgent.isStopped = true;
            blackboard.NavMeshAgent.ResetPath();

            // // 공격 애니메이션 재생
            // if (blackboard.Animator is not null)
            // {
            //     // 재생 중인 모든 애니메이션 초기화
            //     blackboard.Animator.Rebind();
            //     blackboard.Animator.Update(0f);
            //
            //     switch (skillID)
            //     {
            //         // 스킬 ID에 따라 다른 애니메이션 재생
            //         case 4001:
            //             {
            //                 // 스킬 1 사용 (예: 일반 공격)
            //                 blackboard.Animator.SetTrigger("Skill1");
            //                 yield return null;
            //                 AnimatorStateInfo animaState = blackboard.Animator.GetCurrentAnimatorStateInfo(0);
            //                 float animationLength = animaState.length;
            //                 Debug.Log($"Animation length for Skill1: {animationLength} seconds");
            //                 yield return new WaitForSeconds(animationLength);
            //                 break;
            //             }
            //         case 4002:
            //             {
            //                 // 스킬 2 사용 (예: 강력한 공격)
            //                 blackboard.Animator.SetTrigger("Skill2");
            //                 yield return null;
            //                 AnimatorStateInfo animaState = blackboard.Animator.GetCurrentAnimatorStateInfo(0);
            //                 float animationLength = animaState.length;
            //                 Debug.Log($"Animation length for Skill2: {animationLength} seconds");
            //                 yield return new WaitForSeconds(animationLength);
            //                 break;
            //             }
            //         case 4003:
            //             {
            //                 // 스킬 3 사용 (예: 광역 공격)
            //                 blackboard.Animator.SetTrigger("Skill3");
            //                 yield return null;
            //                 AnimatorStateInfo animaState = blackboard.Animator.GetCurrentAnimatorStateInfo(0);
            //                 float animationLength = animaState.length;
            //                 Debug.Log($"Animation length for Skill3: {animationLength} seconds");
            //                 yield return new WaitForSeconds(animationLength);
            //                 break;
            //             }
            //         case 4004:
            //             {
            //                 // 스킬 4 사용 (예: 광역 공격)
            //                 blackboard.Animator.SetTrigger("Skill4");
            //                 yield return null;
            //                 AnimatorStateInfo animaState = blackboard.Animator.GetCurrentAnimatorStateInfo(0);
            //                 float animationLength = animaState.length;
            //                 Debug.Log($"Animation length for Skill4: {animationLength} seconds");
            //                 yield return new WaitForSeconds(animationLength);
            //                 break;
            //             }
            //         default:
            //             Debug.LogWarning($"Unknown skill ID: {skillID}. No animation played.");
            //             yield return null; // 애니메이션이 없을 경우 대기
            //             break;
            //     }
            // }
            
            // 쿨타임 적용

            switch (skillID)
            {
                case 4002:
                    {
                        // 공격 대상 바라보기
                        blackboard.Agent.transform.LookAt(blackboard.Target.transform);
                        // 단발 총알 발사
                        FireBullet(blackboard);
                        break;
                    }
                case 4004:
                    {
                        yield return new WaitForSeconds(0.5f); // 약간의 딜레이 후 시작
                        // 공격 대상 바라보기
                        blackboard.Agent.transform.LookAt(blackboard.Target.transform);
                        
                        // 기 모아서 연발 총알 발사
                        int bulletCount = 5; // 발사할 총알 수
                        float interval = 0.2f; // 총알 발사 간격 (초)
                        for (int i = 0; i < bulletCount; i++)
                        {
                            FireBullet(blackboard);
                            yield return new WaitForSeconds(interval);
                        }
                        break;
                    }
            }
            
            
            if (coolTime > 0f) blackboard.ApplyCooldown(skillID, coolTime + Time.time);
            
            // Agent 움직임 재개
            blackboard.NavMeshAgent.isStopped = false;
            
            // 코루틴 실행 상태 초기화
            // _isAttacking = false;
            blackboard.State = MonsterState.Idle;

            // 명령어 완료 콜백 호출
            onComplete?.Invoke();
        }
        
        private void FireBullet(Blackboard.Blackboard blackboard)
        {
            // 총알 발사 로직 구현
            Debug.Log("Firing bullet at target!");
            // 예: Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            
            var startPos = blackboard.AttackInfo.firePoint.position;
            var targetPos = blackboard.Target.transform.position + Vector3.up * 1.5f; // 타겟의 중심을 향하도록 약간 위로 조정
            var direction = (targetPos - startPos).normalized;
            
            blackboard.AttackInfo.Fire(blackboard.Agent, blackboard.AttackInfo.firePoint.position, Vector3.zero, direction, _skillRowData.GetStat(EStatType.Damage));
        }
    }
}