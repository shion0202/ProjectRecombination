using UnityEngine;

namespace Monster
{
    public enum ActionType
    {
        Spawn,      // 스폰
        Idle,       // 대기
        Attack,     // 공격
        Evasion,    // 회피
        Patrol,     // 수색
        Chase,      // 추적
        Hit,        // 피격
        Death,      // 죽음
    }

    public class Action
    {
        #region 생성자

        public Action(ActionType type, int priority, GameObject target)
        {
            Type = type;
            Priority = priority; // 우선순위를 액션이 가지지 않고 관리하는 Heap 에서 관리한다.
            Target = target;
        }

        #endregion

        #region Getter

        public ActionType Type { get; }
        public int Priority { get; }
        public GameObject Target { get; }

        #endregion
    }
}