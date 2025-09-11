using System;
using System.Collections;
using UnityEngine;

namespace Monster.AI.Command
{
    public class AttackCommand : AICommand
    {
        private readonly RowData _skillRowData;

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
            
            blackboard.State = MonsterState.Attack;
            
            int skillID = (int)_skillRowData.Stats[EStatType.ID].value;
            float coolTime = _skillRowData.Stats[EStatType.CooldownTime].value;
            
            // Agent 움직임 정지
            blackboard.NavMeshAgent.isStopped = true;
            blackboard.NavMeshAgent.ResetPath();

            switch (skillID)
            {
                case 4001:
                    {
                        // 공격 대상 바라보기
                        blackboard.Agent.transform.LookAt(blackboard.Target.transform);
                        // 단발 총알 발사
                        FireBullet(blackboard);
                        break;
                    }
                case 4002:
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
                case 4003:
                    {
                        // 공격 대상 바라보기
                        blackboard.Agent.transform.LookAt(blackboard.Target.transform);
                        // 단발 총알 발사
                        FireBullet(blackboard, 1);
                        break;
                    }
            }
            
            // 스킬 쿨타임 적용
            if (coolTime > 0f) blackboard.ApplyCooldown(skillID, coolTime + Time.time);
            
            // Agent 움직임 재개
            blackboard.NavMeshAgent.isStopped = false;
            
            // 코루틴 실행 상태 초기화
            blackboard.State = MonsterState.Idle;

            // 명령어 완료 콜백 호출
            onComplete?.Invoke();
        }
        
        private void FireBullet(Blackboard.Blackboard blackboard, int bulletType = 0)
        {
            // 총알 발사 로직 구현
#if DEBUG_LOG
            Debug.Log("Firing bullet at target!");
#endif
            Vector3 startPos = blackboard.AttackInfo.firePoint.position;
            Vector3 targetPos = blackboard.Target.transform.position + Vector3.up * 1.5f; // 타겟의 중심을 향하도록 약간 위로 조정
            Vector3 direction = (targetPos - startPos).normalized;
            
            blackboard.AttackInfo.Fire(bulletType, blackboard.Agent, blackboard.AttackInfo.firePoint.position, Vector3.zero, direction, _skillRowData.GetStat(EStatType.Damage));
        }
    }
}