using System;
using Jaeho.Monster;
using UnityEngine;
using UnityEngine.AI;

// 몬스터 스포너는 던전의 특정 위치에 배치되어 몬스터를 생성하는 역할을 한다.
// 몬스터 스포너는 몬스터 프리팹을 가지고 있으며, 하나의 스포너는 한 마리의 몬스터를 스폰한다.
// 몬스터를 스폰한 후 몬스터의 타겟을 플레이어로 설정한다.
// 스포너는 몬스터가 모두 죽었는지 확인하는 기능을 가지고 있다.
// 몬스터의 스폰 여부는 스포너의 활성 여부로 판단한다.


namespace Jaeho.DungeonScript
{
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        [SerializeField] private GameObject monsterPrefab;              // 스폰할 몬스터 프리팹
        [SerializeField] private GameObject monster;
        [SerializeField] private bool isSpawned;                        // 스폰 여부
        [SerializeField] private bool isCleared;                        // 클리어 여부
        

        #region Public Methods

        public void Spawn()
        {
            try
            {
                if (isSpawned) return;
                // monster = instanctiate(monsterPrefab, transform, true);
                monster = Instantiate(monsterPrefab, transform);
                
                if (monster.GetComponent<NavMeshAgent>() == null)
                {
                    Debug.LogWarning("Monster Spawner can't spawn Monster.");
                    monster.AddComponent<NavMeshAgent>();
                }
                
                monster.transform.localPosition = Vector3.zero;
                monster.GetComponent<MonsterBase>().SetTarget(DungeonManager.instance.player);
                isSpawned = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public bool IsClear()
        {
            if (isSpawned && !isCleared )
            {
                if (monster == null)
                    return isCleared = true; // 클리어 상태
                var monsterBase = monster.GetComponent<MonsterBase>();
                if (monsterBase != null && monsterBase.GetMonsterState() == MonsterState.Dead)
                    return isCleared = true; // 클리어 상태
            }
            return isCleared = false;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(2,2,2));
        }

        #endregion
    }
}