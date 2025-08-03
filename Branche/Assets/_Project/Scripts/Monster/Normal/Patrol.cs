using UnityEngine;

namespace Monster.Normal
{
    public class Patrol : State
    {
        public Patrol(AI monsterAI) : base(monsterAI) { }

        public override void OnEnter()
        {
            Debug.Log("Idle State Enter");
            
            // 필요한 컴포넌트가 초기화 되었는지 점검한다.
            if (!AI.CheckProperties())
            {
                Debug.LogWarning(AI.Stats.name + " 캐릭터의 상태값이 정상적으로 초기화 되지 않았습니다.");
                return;
            }
                
            // 몬스터 메니저를 통해 타겟을 불러온다.
            AI.SetTarget();

            if (AI.Target is not null) return;
            Debug.LogWarning("전투 대상을 식별할 수 없습니다.");
        }
        
        public override void OnUpdate()
        {
            // 1. 현재 상황을 분석
            // 2. 가능한 행동들의 우선순위를 계산하여 큐에 추가
            {
                // 죽음 처리 (1순위)
                if (AI.Stats.currentHealth <= 0) AI.AddAction(new Action(ActionType.Death, 100, null));
                // 공격 범위 안에 플레이어가 있으면 전투 (2순위)
                
                // 둘 사이의 거리를 계산한다.
                var distance = Vector3.Distance(AI.Target.transform.position, AI.Body.transform.position);
            
                // 공격 가능한 범위 안에 들어왔는지
                if (distance <= AI.Stats.attackRange) AI.AddAction(new Action(ActionType.Attack, 50, AI.Target));
                
                // 시야 범위에 플레이어가 있으면 추적 (3순위)
                if (distance <= AI.Stats.detectiveRange) AI.AddAction(new Action(ActionType.Chase, 10, AI.Target));
                
                // 모든 조건이 충족되지 않으면 랜덤 인자 생성 
                var rand = Random.Range(0, 5);
                // 반반 확률로 Idle과 Patrol 전환
                AI.AddAction(new Action(ActionType.Idle, 5 - rand, null));
                AI.AddAction(new Action(ActionType.Patrol, rand, null));
            }
        }

        public override void OnExit()
        {
            Debug.Log("Idle State Exit");
        }
    }
}