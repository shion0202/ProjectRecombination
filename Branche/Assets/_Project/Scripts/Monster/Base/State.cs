namespace Monster
{
    // 몬스터의 모든 상태의 기반이 될 추상 클래스
    public abstract class State
    {
        protected AI AI;
        // public int Key;               // 애니메이션 번호

        protected State(AI monsterAI)
        {
            AI = monsterAI;
        }

        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
    }
}