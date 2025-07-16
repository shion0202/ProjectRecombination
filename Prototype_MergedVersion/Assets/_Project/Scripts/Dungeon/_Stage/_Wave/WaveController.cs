using System.Collections.Generic;
using UnityEngine;

namespace Jaeho.DungeonScript
{
    public class WaveController : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private List<GameObject> spawners = new List<GameObject>();
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private bool isCleared = false;

        #region Getters and Setters

        // public List<GameObject> MonsterSpawners => monsterSpawners;
        public bool IsWaveActive
        {
            get => isWaveActive;
            set => isWaveActive = value;
        }
        public bool IsCleared
        {
            get => isCleared;
            set => isCleared = value;
        }

        #endregion

        #region public Methods

        public void SpawnMonsters()
        {
            if (isWaveActive) return;
            
            foreach (var spawner in spawners)
            {
                if (spawner != null)
                {
                    // Assuming each spawner has a method to spawn monsters
                    spawner.GetComponent<MonsterSpawner>().Spawn();
                }
                else
                {
                    Debug.LogWarning("Monster spawner is null.");
                }
            }
            
            isWaveActive = true;
        }

        // public bool ClearWave()
        // {
        //     if (!isWaveActive) return isCleared;
        //     if (isCleared) return isCleared;
        //     foreach (var spawner in spawners)
        //     {
        //         var monsterSpawner = spawner.GetComponent<MonsterSpawner>();
        //         if (monsterSpawner is null || !monsterSpawner.IsAllMonstersDead())
        //         {
        //             // Wave is not cleared yet
        //             break;
        //         }
        //         isCleared = true;
        //         Debug.Log("All monsters are dead. Wave cleared!");
        //     }
        //     return isCleared;
        // }

        public bool CheckWaveClear()
        {
            // 웨이브가 클리어 되었는지 확인
            // 몬스터 스포너가 생성한 모든 몬스터가 죽었는지 확인
            // 만약 모든 몬스터가 죽었다면 true, 아니면 false
            if (spawners.Count == 0)
            {
                Debug.LogWarning("No monster spawners found in the wave.");
                return true;
            }

            if (isCleared) return isCleared;
            
            foreach (var spawner in spawners)
            {
                // spawner.GetComponent<MonsterSpawner>().IsAllMonstersDead();
                var isMonsterDead = spawner.GetComponent<MonsterSpawner>().IsClear();
                if (!isMonsterDead)
                {
                    // If any spawner has monsters alive, the wave is not cleared
                    return isCleared = false;
                }
            }
            return isCleared = true;
        }

        #endregion

        #region Editor Methods

        public void GenerateSpawner()
        {
            var newSpawner = new GameObject("point" + (spawners.Count + 1));
            newSpawner.AddComponent<MonsterSpawner>();
            newSpawner.transform.SetParent(transform);
            newSpawner.transform.localPosition = Vector3.zero;
            
            spawners.Add(newSpawner);
        }

        public void DeleteLastSpawner()
        {
            if (spawners.Count <= 0)
            {
                Debug.LogWarning("No waves to delete.");
                return;
            }
            var spawner = spawners[spawners.Count - 1];
            spawners.RemoveAt(spawners.Count - 1);
            DestroyImmediate(spawner);
        }

        #endregion
    }
}