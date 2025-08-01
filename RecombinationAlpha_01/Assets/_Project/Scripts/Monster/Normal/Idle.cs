using UnityEngine;

// 몬스터가 Idle 상태일 때의 처리
namespace Monster.Normal
{
    public class Idle : State
    {
        public Idle(AI monsterAI) : base(monsterAI) { }

        public override void OnEnter()
        {
            // Idle 상태에 진입하면 재생할 애니메이션을 설정한다.
            // Key = Random.Range(0, 4);
            Debug.Log("Idle State Enter");
            
            // 기본 상태에서는 자리에서 대기한다.
            AI.NavMeshAgent.isStopped = true;
        }
        
        public override void OnUpdate()
        {
            Debug.Log("Idle State Update");
            // 1. 현재 상황을 분석
            // 2. 가능한 행동들의 우선순위를 계산하여 큐에 추가
            // 상태값을 임의로 변경하는 것은 가급적 피해야 한다.
            
            // 죽음 처리 (1순위)
            if (AI.Stats.currentHealth <= 0) AI.AddAction(new Action(ActionType.Death, 100, null));
            
            // 둘 사이의 거리를 계산한다.
            var distance = Vector3.Distance(AI.Target.transform.position, AI.Body.transform.position);
        
            // 좌표를 비교한다. (공격 가능 거리와 같거나 더 짧다면 true, 아니면 false)
            if (distance <= AI.Stats.attackRange) AI.AddAction(new Action(ActionType.Attack, 50, AI.Target));
                
            // 시야 범위에 플레이어가 있으면 추적 (3순위)
            if (distance <= AI.Stats.detectiveRange) AI.AddAction(new Action(ActionType.Chase, 10, AI.Target));
            
            // 모든 조건이 충족되지 않으면 랜덤 인자 생성 
            var rand = Random.Range(0, 5);
            // 반반 확률로 Idle과 Patrol 전환
            if (rand < 3)
                AI.AddAction(new Action(ActionType.Patrol, 5, AI.Target));
            else
            {
                // TODO: 어떻게 해야 부드럽게 애니메이션을 전환할 수 있을까?
                // 다른 애니메이션 끼리 겹치는 구간을 어떻게 관리하면 좋을까?
                AI.AddAction(new Action(ActionType.Idle, 5, AI.Target));
            }
        }

        public override void OnExit()
        {
            Debug.Log("Idle State Exit");
            AI.NavMeshAgent.isStopped = false;
        }
    }
}