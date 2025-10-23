using _Test.Skills;
using Managers;
using Monster.AI.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Monster.AI.FSM
{
    public class MonsterFSM : FSM
    {
        [SerializeField] private GameObject ralphTwoHandsAttackCollider;
        
        #region private Fields
        
        // 상태별 로직에 필요한 내부 변수들
        private float _waitTimer;
        private Skill _useSkill;
        // private bool _isAttack;
        // private int _attackCount;

        // private AmonMeleeCollision _amonMeleeCollision;
        private AmonMeleeCollision _meleeCollision;
        #endregion

        #region Core FSM Methods: Think & Act (Overrided)

        protected override void Init()
        {
            blackboard.Init();
            isInit = true;
        }
        
        /// <summary>
        /// AI의 두뇌 역할: 모든 조건을 검사하여 어떤 상태로 전환할지 결정(판단)합니다.
        /// 기존의 Tink() 메서드와 동일합니다.
        /// </summary>
        protected override void Think()
        {
            if (!isEnabled) return;
            if (blackboard.State.GetStates() == "None")
            {
                ChangeState("Idle");
                return;
            }
            
            if (blackboard.CurrentHealth <= 0)
            {
                if (blackboard.State.GetStates() != "Death") ChangeState("Death");
                return;
            }
            
            // Debug.Log($"Current HP: {blackboard.CurrentHealth}/{blackboard.MaxHealth}");
            
            // 공격이 진행 중일 때는 다른 판단을 하지 않도록 하여 상태를 유지합니다.
            // if (_isAttack)
            // {
            //     if (blackboard.State.GetStates() != "Attack") ChangeState("Attack");
            //     return;
            // }
            if (blackboard.IsAnySkillRunning) 
            {
                // Debug.Log(blackboard.IsAnySkillRunning);
                return; // 스킬이 실행 중이면 상태 전환을 하지 않음
            }
            
            // 사용 가능한 스킬 검사 (우선순위가 가장 높음)
            if (blackboard.Skills is not null && blackboard.Skills.Length != 0)
            {
                foreach (var skill in blackboard.Skills)
                {
                    // int skillId = skill.skillData.skillID;
                    if (skill.CurrentState != Skill.SkillState.isReady) continue;

                    float skillRange = skill.skillData.range;
                    float distanceToPlayer = Vector3.Distance(transform.position, blackboard.Target.transform.position);
                        
                    if (distanceToPlayer <= skillRange)
                    {
                        _useSkill = skill;
                        ChangeState("Attack");
                        return;
                    }
                        
                    // 사거리 밖에 있으면 추격 상태로 전환
                    if (distanceToPlayer > skillRange && distanceToPlayer <= blackboard.MaxDetectionRange)
                    {
                        ChangeState("Chase");
                        return;
                    }
                }
            }
            
            // 플레이어와의 거리 검사
            float distance = Vector3.Distance(transform.position, blackboard.Target.transform.position);
            // if (distance <= blackboard.MaxDetectionRange || blackboard.CurrentHealth < blackboard.MaxHealth)
            // {
            //     // 플레이어가 인지 범위 내에 있으나 최소 사거리 밖에 있는 경우 추격 상태로 전환
            if (distance > blackboard.MinDetectionRange)
            {
                ChangeState("Chase");
                return;
            }
            //     else
            //     {
            //         ChangeState("Idle");
            //     }
            //     return;
            // }
            
            // Manager로 부터 주변에 몬스터가 전투와 관련된 상태인지 확인
            // GameObject[] monsters = MonsterManager.Instance.GetBattleMonsters();
            // foreach (var monster in monsters)
            // {
            //     // 자신이 아닌 몬스터만 검사
            //     if (monster == gameObject) continue;
            //
            //     // 인지 범위 내에 있는지 검사
            //     float dist = Vector3.Distance(transform.position, monster.transform.position);
            //     if (dist > blackboard.MaxDetectionRange) continue;
            //
            //     ChangeState("Chase");
            //     return;
            // }

            if (blackboard.PatrolInfo.wayPoints is { Length: > 0 })
            {
                // 위의 모든 조건이 아닐 경우, 기본 행동(순찰 또는 대기)으로 돌아갑니다.
                if (blackboard.PatrolInfo.isPatrol)
                {
                    ChangeState("Patrol");
                    return;
                }

                if (_waitTimer <= 0f)
                {
                    ChangeState("Patrol");
                    _waitTimer = Random.Range(3f, 7f);
                }
            }
            else
            {
                ChangeState("Idle");
                _waitTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// AI의 몸 역할: 현재 상태(State)에 따라 실제 행동을 수행합니다.
        /// 기존의 모든 Handle...State() 메서드를 통합했습니다.
        /// </summary>
        protected override void Act()
        {
            if (!isEnabled) return;
            if (blackboard?.State is null) return;

            if (blackboard.IsAnySkillRunning)
            {
                // Debug.Log(blackboard.IsAnySkillRunning);
                return; // 스킬이 실행 중이면 상태 전환을 하지 않음
            }
            
            string stateName = blackboard.State?.GetStates() ?? "None";
            
            switch (stateName)
            {
                case "None":
                    // 아무 것도 하지 않음
                    break;
                case "Idle":
                    // Idle 상태에서는 특별한 행동이 없으므로 EnterState에서 처리한 isStopped = true가 유지됩니다.
                    break;
                case "Death":
                    ActDeath();
                    break;
                case "Patrol":
                    ActPatrol();
                    break;
                case "Chase":
                    ActChase();
                    break;
                case "Attack":
                    ActAttack();
                    break;
            }
        }
        
        // 상태 진입 시 1회 호출되는 초기화 메서드 (기존 코드와 동일)
        protected override void EnterState(string stateName)
        {
            InitAnimationFlags();
            blackboard.NavMeshAgent.isStopped = false;

            switch (stateName)
            {
                case "Idle":
                    blackboard.NavMeshAgent.isStopped = true;
                    break;
                case "Death":
                    blackboard.AgentCollider.enabled = false;
                    blackboard.AgentRigidbody.isKinematic = true;
                    blackboard.NavMeshAgent.isStopped = true;
                    blackboard.NavMeshAgent.ResetPath();
                    blackboard.AnimatorParameterSetter.Animator.SetBool("IsDeath", true);
                    break;
                case "Patrol":
                    blackboard.PatrolInfo.isPatrol = true;
                    blackboard.PatrolInfo.CurrentWayPointIndex = blackboard.PatrolInfo.GetNextWayPointIndex();
                    blackboard.NavMeshAgent.SetDestination(blackboard.PatrolInfo.GetCurrentWayPoint());
                    blackboard.NavMeshAgent.speed = blackboard.WalkSpeed;
                    blackboard.AnimatorParameterSetter.Animator.SetBool("IsWalk", true);
                    break;
                case "Chase":
                    blackboard.NavMeshAgent.speed = blackboard.RunSpeed;
                    blackboard.AnimatorParameterSetter.Animator.SetBool("IsRun", true);
                    break;
                case "Attack":
                    blackboard.NavMeshAgent.isStopped = true;
                    break;
            }
        }
        
        #endregion

        #region State Actions (Act에서 호출되는 행동 함수들)

        private void ActDeath()
        { 
            // 1. 죽음 애니메이션을 가지고 있는지 확인
            // foreach (var state in blackboard.AnimatorParameterSetter.BoolParameterNames)
            // {
            //     if (state == "IsDeath")
            //     {
            //         AnimatorStateInfo animatorStateInfo = blackboard.AnimatorParameterSetter.CurrentAnimatorStateInfo;
            //         if (animatorStateInfo.IsName("Death") && animatorStateInfo.normalizedTime >= 1.0f)
            //         {
            //             blackboard.DeathEffect?.SetActive(true);
            //             isInit = false;
            //             gameObject.SetActive(false);
            //         }
            //         return; // 죽음 애니메이션이 있으면 여기서 종료
            //     }
            // }
            
            blackboard.AnimatorParameterSetter.Animator.SetTrigger("Death");
            
            blackboard.NavMeshAgent.isStopped = true;
            blackboard.AgentCollider.enabled = false;
            blackboard.AgentRigidbody.isKinematic = true;
            
            // 2. 죽음 이팩트가 있는지 확인
            if (blackboard.DeathEffect is not null)
            {
                var effect = blackboard.DeathEffect;
                
                // 이팩트가 있으면 활성화 시키고 이팩트가 종료 될때까지 대기
                effect.SetActive(true);
                var particleSystem = effect.GetComponent<ParticleSystem>();
                if (particleSystem is not null)
                {
                    if (!particleSystem.isPlaying)
                        particleSystem.Play();
                    
                    StartCoroutine(WaitForParticleEnd(particleSystem));
                }
            }
        }

        private void ActPatrol()
        {
            // 목표 지점에 도착했는지 확인하고 다음 행동을 결정합니다.
            if (blackboard.NavMeshAgent.remainingDistance <= blackboard.NavMeshAgent.stoppingDistance && !blackboard.NavMeshAgent.pathPending)
            {
                blackboard.PatrolInfo.isPatrol = false; // isPatrol을 false로 만들어 Think()가 다음 판단(대기 또는 새 순찰)을 하도록 유도
                blackboard.AnimatorParameterSetter.Animator.SetBool("IsWalk", false);
            }
        }

        private void ActChase()
        {
            // 추격 상태의 행동: 매 프레임 타겟의 위치로 목적지를 갱신합니다.
            blackboard.NavMeshAgent.SetDestination(blackboard.Target.transform.position);
            
            var distance = Vector3.Distance(transform.position, blackboard.Target.transform.position);
            if (distance <= blackboard.MinDetectionRange)
            {
                blackboard.AnimatorParameterSetter.Animator.SetBool("IsRun", false);
            }
        }

        private void ActAttack()
        {
            if (_useSkill is null || blackboard.Target is null) return;
            
            // 타겟을 바라보게 합니다.
            transform.LookAt(blackboard.Target.transform);
            
            _useSkill.Execute(blackboard);
        }

        #endregion

        #region Helper & Event Methods
        
        private void FireBullet(int bulletType = 0)
        {
            Vector3 startPos = blackboard.AttackInfo.firePoint.position;
            Vector3 targetPos = blackboard.Target.transform.position + Vector3.up * 1.5f;
            Vector3 direction = (targetPos - startPos).normalized;
            
            blackboard.AttackInfo.Fire(bulletType, blackboard.Agent, blackboard.AttackInfo.firePoint.position, Vector3.zero, direction, _useSkill.skillData.damage);
        }
        
        public void AnimationEvent_Fire()
        {
            if (_useSkill.skillData.skillID is 4003 or 4002)
                FireBullet(1);
            else
                FireBullet();
        }
        
        public void AnimationEvent_Melee()
        {
            if (blackboard.Target == null || _useSkill == null) return;

            float damage = _useSkill.skillData.damage;
            var amonMeleeCollision = Utils.Instantiate(ralphTwoHandsAttackCollider, blackboard.Agent.transform);
            _meleeCollision = amonMeleeCollision.GetComponent<AmonMeleeCollision>();
            if (_meleeCollision)
            {
                _meleeCollision.Init(damage, new Vector3(4f,4f,4f), new Vector3(1f,1f,2f));
            }
        }
        
        public void OnAttackAnimationEnd()
        {
            // _isAttack = false;
            // _useSkill = null; // 스킬 사용 완료
            if (_meleeCollision)
            {
                Utils.Destroy(_meleeCollision.gameObject);
            }
            ChangeState("Idle");
        }

        private IEnumerator WaitForParticleEnd(ParticleSystem ps)
        {
            if (ps is null) yield break;

            // 파티클 시스템이 재생 중일 때까지 대기
            yield return new WaitForSeconds(ps.main.duration);

            // 파티클 시스템이 끝난 후 오브젝트 비활성화
            isInit = false;
            // gameObject.SetActive(false);
            PoolManager.Instance.ReleaseObject(gameObject);
        }

        #endregion
    }
}