using Monster.AI.Blackboard;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    public class MonsterManager: Singleton<MonsterManager>
    {
        [SerializeField] private List<GameObject> monsters;
        public GameObject Player { get; set; }
        
        public void AddMonster(GameObject monster)
        {
            if (monster is not null && !monsters.Contains(monster))
            {
                monsters.Add(monster);
            }
        }
        
        public void RemoveMonster(GameObject monster)
        {
            if (monster is not null && monsters.Contains(monster))
            {
                monsters.Remove(monster);
            }
        }
        
        public void ReleaseAllMonsters()
        {
            foreach (GameObject monster in monsters.Where(monster => monster is not null))
            {
                PoolManager.Instance.ReleaseObject(monster);
            }

            monsters.Clear();
        }

        /// <summary>
        /// 한 판이 끝났을 때 몬스터 참조를 정리한다.
        /// 씬 언로드 "전에" 호출해야 한다. 대여 중인 풀 오브젝트가 씬과 함께 파괴되면
        /// 풀은 여전히 대여 중으로 알고 있는데 실체가 사라져 다음 판에서 null이 튄다.
        /// </summary>
        public void ResetSession()
        {
            ReleaseAllMonsters();
            Player = null;

            Debug.Log("[MonsterManager] 세션 리셋 완료");
        }

        public GameObject[] GetBattleMonsters()
        {
            List<GameObject> battleMonsters = new();
            foreach (GameObject monster in monsters)
            {
                Blackboard blackboard = monster?.GetComponentInChildren<Blackboard>();
                if (blackboard is not null)
                {
                    if (blackboard.State.HasState("Chase") || blackboard.State.HasState("Attack") || blackboard.State.HasState("Hit"))
                    {
                        battleMonsters.Add(monster);
                    }
                }

            }
            return battleMonsters.ToArray();
        }

        public void PauseMonsters()
        {
            // To-do: Monster AI를 정지시키는 로직 (카메라 컷씬 등 필요한 상황에서 Game Manager에 의해 호출되는 함수)
        }

        public void UnpauseMonsters()
        {
            // To-do: Monster AI를 다시 실행시키는 로직
            // 함수 2개 쓰는 게 불편하다면 bool 등으로 함수 하나로 통일하고 Game Manager 쪽만 수정해주시면 됩니다
        }
    }
}
