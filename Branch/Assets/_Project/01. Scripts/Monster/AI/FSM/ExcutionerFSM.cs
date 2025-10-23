using _Test.Skills;
using Monster.AI.FSM;
using System.Collections;
using UnityEngine;

public class ExcutionerFSM : FSM
{
    [SerializeField] private GameObject spawnModel;
    [SerializeField] private GameObject bodyModel;
    
    // Init
    protected override void Init()
    {
        if (isInit) return;
        if (blackboard is null)
        {
            Debug.LogError("Blackboard is null in ExcutionerFSM");
            return;
        }

        blackboard.Init();
        
        InitAnimationFlags();
        
        // NavMesh초기화
        if (blackboard.NavMeshAgent != null)
        {
            blackboard.NavMeshAgent.enabled = true;
            blackboard.NavMeshAgent.isStopped = true; // 초기에는 멈춘 상태로 시작
            blackboard.NavMeshAgent.ResetPath();
        }
        
        ChangeState("Spawn");
        isInit = true;
    }
    
    // 망나니 FSM
    protected override void Think()
    {
        if (!isEnabled) return;
        if (blackboard?.Target is null)
        {
            Debug.Log("Target is null");
            return;
        }
        if (blackboard.State.GetStates() == "Death")
        {
            Debug.Log("State is Death");
            return;
        }
        Debug.Log($"Current Health: {blackboard.CurrentHealth}");
        if (blackboard.CurrentHealth <= 0)
        {
            ChangeState("Death");
            return;
        }
        // ============================================
        if (blackboard.IsAnySkillRunning) 
        {
            Debug.Log(blackboard.IsAnySkillRunning);
            return; // 스킬이 실행 중이면 상태 전환을 하지 않음
        }
        // ============================================

        float distance = Vector3.Distance(blackboard.transform.position, blackboard.Target.transform.position);
        
        if (blackboard.Skills[0].CurrentState == Skill.SkillState.isReady && distance <= blackboard.Skills[0].skillData.range)
        {
            ChangeState("UsingSkill1");
        }
        else if (blackboard.Skills[1].CurrentState == Skill.SkillState.isReady && distance <= blackboard.Skills[1].skillData.range)
        {
            ChangeState("UsingSkill2");
        }
        else if (blackboard.Skills[2].CurrentState == Skill.SkillState.isReady && distance <= blackboard.Skills[2].skillData.range)
        {
            ChangeState("UsingSkill3");
        }
        else if (blackboard.Skills[3].CurrentState == Skill.SkillState.isReady && distance <= blackboard.Skills[3].skillData.range)
        {
            ChangeState("UsingSkill4");
        }
        else if (blackboard.Skills[4].CurrentState == Skill.SkillState.isReady && distance <= blackboard.Skills[4].skillData.range)
        {
            ChangeState("UsingSkill5");
        }
        else if (blackboard.MinDetectionRange < distance)
        {
            ChangeState("Chase");
        }
        else
        {
            ChangeState("Idle");
        }
    }

    protected override void Act()
    {
        if (!isEnabled)
        {
            ActSpawn();
            return;
        }
        if (blackboard?.Target is null) return;
        
        string state = blackboard.State.GetStates();
        if (state is null) return;
        
        if (blackboard.IsAnySkillRunning) 
        {
            Debug.Log(blackboard.IsAnySkillRunning);
            return; // 스킬이 실행 중이면 상태 전환을 하지 않음
        }
        
        switch (state)
        {
            case "Idle":
                // 대기 상태에서 특별한 행동이 필요하지 않음
                break;
            case "UsingSkill1":
                blackboard.Skills[0].Execute(blackboard);
                break;
            case "UsingSkill2":
                blackboard.Skills[1].Execute(blackboard);
                break;
            case "UsingSkill3":
                blackboard.Skills[2].Execute(blackboard);
                break;
            case "UsingSkill4":
                blackboard.Skills[3].Execute(blackboard);
                break;
            case "UsingSkill5":
                blackboard.Skills[4].Execute(blackboard);
                break;
            case "Chase":
                ActChase();
                break;
            case "Death":
                // 사망 처리 로직
                ActDeath();
                break;
        }
    }

    private void ActDeath()
    {
        if (blackboard.Agent is null) return;
        // 사망 시 NavMeshAgent 멈추기
        blackboard.NavMeshAgent.isStopped = true;
        blackboard.AnimatorParameterSetter.Animator.SetTrigger("Death");
        StartCoroutine(WaitDeathAnimation());
    }

    private IEnumerator WaitDeathAnimation()
    {
        yield return new WaitForSeconds(5f);
        
        Destroy(gameObject);
    }

    private void ActChase()
    {
        if (blackboard.Agent is null || blackboard.Target is null) return;
        
        // NavMeshAgent를 사용하여 플레이어를 추적
        blackboard.NavMeshAgent.SetDestination(blackboard.Target.transform.position);
    }

    private void ActSpawn()
    {
        if (blackboard.Agent is null) return;
        
        // 스폰 시 NavMeshAgent 멈추기
        blackboard.NavMeshAgent.isStopped = true;
        
        spawnModel.SetActive(true);
        bodyModel.SetActive(false);
        
        // 스폰 애니메이션 재생
        StartCoroutine(WaitSpawnAnimation());
    }
    
    private IEnumerator WaitSpawnAnimation()
    {
        // blackboard.AnimatorParameterSetter.Animator.SetBool("isSpawn", true);
        yield return new WaitForSeconds(6f);
        
        spawnModel.SetActive(false);
        bodyModel.SetActive(true);
        
        isEnabled = true; // FSM 활성화
        ChangeState("Idle");
    }

    protected override void EnterState(string stateName)
    {
        switch (stateName)
        {
            case "Idle":
                blackboard.NavMeshAgent.isStopped = true;
                break;
            case "UsingSkill1":
                blackboard.NavMeshAgent.isStopped = true;
                break;
            case "UsingSkill2":
                blackboard.NavMeshAgent.isStopped = true;
                break;
            case "UsingSkill3":
                blackboard.NavMeshAgent.isStopped = true;
                break;
            case "UsingSkill4":
                blackboard.NavMeshAgent.isStopped = true;
                break;
            case "UsingSkill5":
                blackboard.NavMeshAgent.isStopped = true;
                break;
            case "Chase":
                blackboard.NavMeshAgent.isStopped = false;
                break;
            case "Death":
                blackboard.NavMeshAgent.isStopped = true;
                break;
        }
    }
}
