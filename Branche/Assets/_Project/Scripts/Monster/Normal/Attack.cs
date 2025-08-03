using UnityEngine;

namespace Monster.Normal
{
    public class Attack : State
    {
        public Attack(AI monsterAI) : base(monsterAI) { }

        public override void OnEnter()
        {
            Debug.Log("Idle State Enter");
            
            // 공격 시 이동을 중지한다.
            AI.NavMeshAgent.isStopped = true;
        }
        
        public override void OnUpdate()
        {
            // 1. 현재 상황을 분석
            // 2. 가능한 행동들의 우선순위를 계산하여 큐에 추가
            
            if (!AI.IsActing)
            {
                // attack 애니메이션 재생
                AI.Animator.SetInteger("State", 4);
                AI.Animator.SetInteger("AttackKey", Random.Range(1, 4));
            }
            
            {
                // 죽음 처리 (1순위)
                if (AI.Stats.currentHealth <= 0) AI.AddAction(new Action(ActionType.Death, 100, null));
                // 공격 범위 안에 플레이어가 있으면 전투 (2순위)
                
                // 둘 사이의 거리를 계산한다.
                var distance = Vector3.Distance(AI.Target.transform.position, AI.Body.transform.position);
            
                // 좌표를 비교한다. (공격 가능 거리와 같거나 더 짧다면 true, 아니면 false)
                if (distance <= AI.Stats.attackRange) AI.AddAction(new Action(ActionType.Attack, 50, AI.Target));
                
                // 시야 범위에 플레이어가 있으면 추적 (3순위)
                if (distance <= AI.Stats.detectiveRange) AI.AddAction(new Action(ActionType.Chase, 10, AI.Target));
            }
        }

        public override void OnExit()
        {
            Debug.Log("Idle State Exit");
            // 공격이 끝나면 다시 이동을 가능하게 한다.
            AI.NavMeshAgent.isStopped = false;
        }
    }
}