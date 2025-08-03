using UnityEngine;

namespace Monster.Normal
{
    public class Death : State
    {
        public Death(AI monsterAI) : base(monsterAI) { }
        
        public override void OnEnter()
        {
            Debug.Log("Death State Enter");
            
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
            // 몬스터 내부에 동작 중인 모든 코루틴 정지
            if (AI.IsActing) return;
        }

        public override void OnExit()
        {
            
        }
    }
}