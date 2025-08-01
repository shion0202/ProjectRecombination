using UnityEngine;

namespace Monster.Normal
{
    public class Hit : State
    {
        public Hit(AI monsterAI) : base(monsterAI) { }

        public override void OnEnter()
        {
            
        }
        
        public override void OnUpdate()
        {
            // 몬스터 내부에 동작 중인 모든 코루틴 정지
            if (AI.IsActing) return;

            // 피격 상태가 끝나면 Idle 상태로 전환
            AI.ChangeState(new Chase(AI));
        }
        
        public override void OnExit()
        {
            // 피격 상태 종료 후 필요한 후처리 작업이 있다면 여기에 작성
            Debug.Log("Hit State Exit");
        }
    }
}