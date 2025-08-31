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
        
        public List<GameObject> Monsters => monsters;
        
        public void AddMonster(GameObject monster)
        {
            if (monster is not null && !monsters.Contains(monster))
            {
                monsters.Add(monster);
            }
        }
        
        // 자신(몬스터)의 근처에서 싸움이 일어났는지 확인하는 메서드 (BT
        public bool IsNearMonsterBattle(GameObject monster, float distance)
        {
            // 몬스터가 null이거나 리스트에 없다면 false 반환
            if (monster is null || !monsters.Contains(monster) || distance <= 0f) return false;
            
            // 메니저가 관리 중인 몬스터가 2 미만 인경우
            if (monsters.Count < 2) return false;
            
            // 몬스터 리스트(monsters)를 순회하며 monster와 거리가 distance 이하인 다른 몬스터가 있는지 확인
            var standardPosition = monster.transform.position;
            
            foreach (var otherMonster in monsters)
            {
                if (otherMonster is null) continue; // null 체크
                // 다른 몬스터의 위치를 가져옴
                var otherPosition = otherMonster.transform.position;
                
                // 다른 몬스터가 자기 자신이 아니고, distance 이하의 거리에 있는지 확인
                if (otherMonster == monster || !(Vector3.Distance(standardPosition, otherPosition) <= distance)) continue;
                
                // 몬스터의 상태가 전투와 관련된 상태인 경우 (Chase, Attack 등)
                var blackboard = otherMonster.GetComponentInChildren<Blackboard>();
                if (blackboard == null) continue;
                if (blackboard.State != MonsterState.Chase && blackboard.State != MonsterState.Attack &&
                    blackboard.State != MonsterState.Hit) continue;
                return true; // 근처에 전투 중인 몬스터가 있음
            }
            return false; // 근처에 전투 중인 몬스터가 없음
        }
        
        /// <summary>
        /// 몬스터 인스턴스 관리(풀링)
        /// </summary>
        

        private void Update()
        {
            for (int i = 0; i < monsters.Count; ++i)
            {
                GameObject monster = monsters[i];
                AIController aiController = monster.GetComponent<AIController>();
                if (aiController is null) continue;
                Blackboard blackboard = aiController.Blackboard;
                if (blackboard is null) continue;
                if (blackboard.State == MonsterState.Death)
                {
                    monster.transform.position = new Vector3(999, 999, 999);
                    // IsDestroy 기믹을 위해 임시로 Destroy로 변경
                    //monster.SetActive(false);
                    monsters.Remove(monster);
                    Destroy(monster);
                    --i;
                }
            }
        }
    }
}