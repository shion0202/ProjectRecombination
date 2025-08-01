using UnityEngine;

namespace Monster.Normal
{
    public class Chase : State
    {
        public Chase(AI monsterAI) : base(monsterAI) { }

        public override void OnEnter()
        {
            Debug.Log("Idle State Enter");
            AI.NavMeshAgent.isStopped = false;
        }
        
        public override void OnUpdate()
        {
            // 1. 현재 상황을 분석
            // 2. 가능한 행동들의 우선순위를 계산하여 큐에 추가
            
            if (!AI.IsActing)
            {
                // Run 애니메이션 재생
                AI.Animator.SetInteger("State", 2);
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

                if (AI.IsActing) return;
                
                // 이동 목적지를 플레이어의 위치로 설정
                AI.NavMeshAgent.speed = AI.Stats.runSpeed;                      // 달리기 속도로 추적 설정
                AI.NavMeshAgent.SetDestination(AI.Target.transform.position);   // 플레이어의 위치로 이동 목적지 설정
            }
        }

        public override void OnExit()
        {
            Debug.Log("Idle State Exit");
            AI.NavMeshAgent.isStopped = true;
        }
    }
}