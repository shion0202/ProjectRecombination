using Managers;
using Monster.AI.FSM;
using UnityEngine;

namespace Managers
{
    public class DungeonManager : Singleton<DungeonManager>
    {
        [Header("보스 활성화 트리거")]
        [SerializeField] private Collider bossActivateTrigger;
        
        [Header("아몬 스킬 설정: 영혼 구체")]
        public GameObject soulSpherePrefab;
        public int maxSoulSpheres = 5;
        public Vector2 spawnAreaMin = new Vector2(-10f, -10f);
        public Vector2 spawnAreaMax = new Vector2(10f, 10f);

        [Header("아몬 2페이즈 설정")]
        public GameObject amonSecondPhasePrefab;
        public Transform amonSpawnPoint;
        public Transform playerTeleportPoint;
        public Transform playerRespawnPoint;

        [SerializeField] private FSM amonFirstPhase;
        public void AmonFirstPhase()
        {
            amonFirstPhase.isEnabled = true;
        }

        private void OnDrawGizmos()
        {
            // 생성 범위 시각화
            Gizmos.color = Color.cyan;
            Vector3 areaCenter = (spawnAreaMin + spawnAreaMax) / 2f;
            Vector3 areaSize = new Vector3(spawnAreaMax.x - spawnAreaMin.x, 0.1f, spawnAreaMax.y - spawnAreaMin.y);
            Gizmos.DrawWireCube(areaCenter, areaSize);
        }
        
        public void SpawnSoulSphere()
        {
            if (soulSpherePrefab == null)
            {
                Debug.LogWarning("soulSpherePrefab이 할당되지 않았습니다.");
                return;
            }
            
            for (int count = 0; count < maxSoulSpheres; count++)
            {
                // 랜덤 위치 생성
                float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
                float randomZ = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
                Vector3 spawnPosition = new Vector3(randomX, 0f, randomZ); // y축은 0으로 고정

                Instantiate(soulSpherePrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"영혼 구체 생성 위치: {spawnPosition}");
            }
        }
    }
}
