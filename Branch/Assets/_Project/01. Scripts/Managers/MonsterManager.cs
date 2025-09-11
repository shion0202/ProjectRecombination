using Monster;
using Monster.AI;
using Monster.AI.Blackboard;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Managers
{
    public class MonsterManager: Singleton<MonsterManager>
    {
        [SerializeField] private List<GameObject> monsters;     // 몬스터가 인스턴스 되면 추가되는 리스트
        [SerializeField] private GameObject player;
        
        public GameObject Player => player;
        
        // public List<GameObject> Monsters => monsters;
        
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
        
        public GameObject[] GetBattleMonsters()
        {
            List<GameObject> battleMonsters = new ();
            foreach (GameObject monster in monsters)
            {
                Blackboard blackboard = monster?.GetComponentInChildren<Blackboard>();
                if (blackboard?.State is MonsterState.Chase or MonsterState.Attack or MonsterState.Hit)
                {
                    battleMonsters.Add(monster);
                }
            }
            return battleMonsters.ToArray();
        }
    }
}