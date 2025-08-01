using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Project.Scripts.VisualScripting
{
    public class Instance : ProcessBase
    {
        [Tooltip("생성할 게임 오브젝트")] [SerializeField]
        private GameObject instancePrefab; // Prefab to instantiate

        [Tooltip("한번에 생성할 수량")] [SerializeField]
        private int count = 1;

        [Tooltip("생성 범위")] [SerializeField] private Vector3 spawnArea;

        [FormerlySerializedAs("_destroyTrigger")]
        [Tooltip("생성한 객체가 파괴 되었음을 감지할 객체")]
        [SerializeField] private IsDestroy destroyTrigger;

        public override void Execute()
        {
            if (IsOn) return;

            if (instancePrefab is null)
            {
                Debug.LogError("Instance prefab is not assigned.");
                return;
            }

            IsOn = true;
            // Instance logic here, e.g., instantiate a prefab or create an object
            Debug.Log("Instance executed, IsOn: " + IsOn);

            // TODO: count 값이 1이상 들어왔을 때 위치 값을 어떻게 줄것인지 고려해야 한다.
            for (var i = 0; i < count; i++)
            {
                // 부모 오브젝트의 위치를 기준으로 무작위 위치 계산
                var randomX = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
                var randomY = Random.Range(-spawnArea.y / 2, spawnArea.y / 2); // 2D라면 0 또는 고정값, 3D라면 필요한 범위
                var randomZ = Random.Range(-spawnArea.z / 2, spawnArea.z / 2);

                var randomPosition = transform.position + new Vector3(randomX, randomY, randomZ);

                var obj = Instantiate(instancePrefab, transform);
                obj.transform.position = randomPosition;

                destroyTrigger?.AddObject(obj.GetComponent<GlobalGameObject>());
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(gameObject.transform.position, spawnArea);
        }
    }
}