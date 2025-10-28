using _Test.Skills;
using Managers;
using Monster.AI.BehaviorTree.Nodes;
using System.Collections;
using UnityEngine;

namespace Monster.AI.FSM
{
    public class FlyFSM : FSM
    {
        [SerializeField] private GameObject attackCollider;
        private AmonMeleeCollision _attackCollider;

        private bool _isDeath;

        private Skill _useSkill;

        protected override void Init()
        {
            blackboard.Init();
            isInit = true;
        }
        
        protected override void Think()
        {
            if (!isEnabled || _isDeath) return;
            
            Debug.Log($"currentState: {blackboard.State.GetStates()}");
            
            if (blackboard.State.GetStates() == "None")
            {
                ChangeState("Idle");
                return;
            }

            // 0 순위: 사망 체크
            if (blackboard.CurrentHealth <= 0)
            {
                ChangeState("Death");
                return;
            }
            
            // 1 순위: 스킬 사용 여부 체크
            if (blackboard.IsAnySkillRunning)
            {
                if (_isDeath)
                {
                    blackboard.StopAllCoroutines();
                }
                else
                {
                    float f = Vector3.Distance(blackboard.Agent.transform.position, blackboard.Target.transform.position);
                    if (_useSkill is { CurrentState: not Skill.SkillState.isReady and Skill.SkillState.isEnded } &&
                        f > _useSkill.skillData.range)
                    {
                        blackboard.StopAllCoroutines();
                        _useSkill = null;
                        ChangeState("Chase");
                    }
                }

                return;
            }
            
            // 2 순위: 스킬 쿨타임 및 거리 체크 후 스킬 사용
            if (blackboard.Skills is not null && blackboard.Skills.Length > 0)
            {
                // 스킬 0 우선 사용
                if (blackboard.Skills[0].CurrentState == Skill.SkillState.isReady)
                {
                    float skillRange = blackboard.Skills[0].skillData.range;
                    float distanceToTarget = Vector3.Distance(blackboard.Agent.transform.position, blackboard.Target.transform.position);
                    
                    if (distanceToTarget <= skillRange)
                    {
                        _useSkill = blackboard.Skills[0];
                        ChangeState("UsingSkill1");
                        return;
                    }
                }
                
                // 스킬 1 다음으로 사용
                if (blackboard.Skills[1].CurrentState == Skill.SkillState.isReady)
                {
                    float skillRange = blackboard.Skills[1].skillData.range;
                    float distanceToTarget = Vector3.Distance(blackboard.Agent.transform.position, blackboard.Target.transform.position);
                    
                    if (distanceToTarget <= skillRange)
                    {
                        _useSkill = blackboard.Skills[1];
                        ChangeState("UsingSkill2");
                        return;
                    }
                }
            }
            
            // 3 순위: 플레이어와의 거리 체크 후 추적 또는 대기 상태로 전환
            float distance = Vector3.Distance(blackboard.Agent.transform.position, blackboard.Target.transform.position);
            if (distance > blackboard.MinDetectionRange)
            {
                
                ChangeState("Chase");
                return;
            }
            
            ChangeState("Idle");
        }

        protected override void Act()
        {
            if (!isEnabled || _isDeath || blackboard?.State is null || blackboard.IsAnySkillRunning) return;
            
            string state = blackboard.State.GetStates() ?? "None";

            switch (state)
            {
                case "None":
                    break;
                case "Idle":
                    ActIdle();
                    break;
                case "Death":
                    ActDeath();
                    break;
                case "Chase":
                    ActChase();
                    break;
                case "UsingSkill1":
                    blackboard.Skills[0].Execute(blackboard);
                    break;
                case "UsingSkill2":
                    blackboard.Skills[1].Execute(blackboard);
                    break;
            }
        }

        private void ActChase()
        {
            blackboard.AnimatorParameterSetter.Animator.SetBool("isMoving", true);
            blackboard.NavMeshAgent.speed = blackboard.RunSpeed;
            blackboard.NavMeshAgent.SetDestination(blackboard.Target.transform.position);
            blackboard.NavMeshAgent.isStopped = false;
        }

        private void ActDeath()
        {
            if (_isDeath) return;
            _isDeath = true;
            
            blackboard.NavMeshAgent.isStopped = true;
            blackboard.AnimatorParameterSetter.Animator.SetTrigger("Death");
            
            if (blackboard.AgentCollider is not null)
            {
                blackboard.AgentCollider.enabled = false;
            }

            if (blackboard.AgentRigidbody is not null)
            {
                blackboard.AgentRigidbody.isKinematic = false;
            }
            
            if (blackboard.DeathEffect is not null)
            {
                var effect = blackboard.DeathEffect;
                
                // 이팩트가 있으면 활성화 시키고 이팩트가 종료 될때까지 대기
                effect.SetActive(true);
                var particleSystem = effect.GetComponent<ParticleSystem>();
                if (particleSystem is null) return;

                if (!particleSystem.isPlaying)
                    particleSystem.Play();
            }
        }

        private void ActIdle()
        {
            throw new System.NotImplementedException();
        }

        protected override void EnterState(string stateName)
        {
            if (!isEnabled || _isDeath) return;
            
            InitAnimationFlags();
            blackboard.NavMeshAgent.isStopped = false;

            switch (stateName)
            {
                case "Idle":
                    blackboard.NavMeshAgent.isStopped = true;
                    break;
            }
        }

        #region Animation Event Methods

        public void AnimationEvent_MeleeAttack()
        {
            if (blackboard.Target is null || _useSkill is null) return;
            
            float damage = _useSkill.skillData.damage;

            GameObject meleeCollider = Utils.Instantiate(attackCollider, blackboard.Agent.transform);
            _attackCollider = meleeCollider.GetComponent<AmonMeleeCollision>();
            if (_attackCollider)
                _attackCollider.Init(damage, new Vector3(2f, 2f, 2f), new Vector3(0f, 2f, 1f));
            
            StartCoroutine(MeleeCollisionDisableDelay());
        }

        private IEnumerator MeleeCollisionDisableDelay()
        {
            yield return new WaitForSeconds(0.2f);
            if (_attackCollider)
            {
                Utils.Destroy(_attackCollider.gameObject);
            }
        }
        
        public void AnimationEvent_Death()
        {
            // 파티클이 재생 중일 수 있으므로, 파티클도 함께 비활성화합니다.
            if (blackboard.DeathEffect is not null)
            {
                blackboard.DeathEffect.SetActive(false);
            }
            
            isInit = false;
            // gameObject.SetActive(false); // 풀 매니저를 사용하므로 이쪽을 권장
            PoolManager.Instance.ReleaseObject(gameObject);
        }

        #endregion
    }
}