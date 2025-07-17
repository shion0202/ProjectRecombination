using System.Collections.Generic;
using UnityEngine;

namespace Jaeho.DungeonScript
{
    public class StageController : MonoBehaviour
    {
        [Header("Stage Settings")]
        [SerializeField] private List<GameObject> waves = new List<GameObject>();
        // [SerializeField] private bool _isPlayerInStage;
        [SerializeField] private int _currentWaveIndex = 0;
        [SerializeField] private bool _isStageCleared = false;
        [SerializeField] private StageType _stageType;
        [SerializeField] private bool isReadyToSpawn;
        // [SerializeField] private bool isInteract;

        #region Getters and Setters
        
        public int currentWaveIndex
        {
            get => _currentWaveIndex;
            set => _currentWaveIndex = value;
        }

        public bool isStageCleared
        {
            get => _isStageCleared;
            set => _isStageCleared = value;
        }
        public StageType stageType
        {
            get => _stageType;
            set => _stageType = value;
        }

        #endregion
        
        #region Editor Methods

        public void AddLastWave()
        {
            var newWave = new GameObject("Wave" + (waves.Count + 1));
            newWave.AddComponent<WaveController>();
            newWave.transform.SetParent(transform);
            
            waves.Add(newWave);
        }

        public void RemoveLastWave()
        {
            if (waves.Count <= 0)
            {
                Debug.LogWarning("No waves to remove.");
                return;
            }
            var lastWave = waves[^1];
            waves.RemoveAt(waves.Count - 1);
            DestroyImmediate(lastWave);
        }
        
        #endregion
        
        #region Unity Methods

        private void Start()
        {
            if (waves.Count <= 0)
            {
                Debug.LogWarning("스테이지에 웨이브 데이터가 없습니다. 웨이브를 추가해주세요.");
            }
            
            if (isReadyToSpawn)
            {
                // 기획자에 의해 몬스터가 미리 스폰되어 있길 바라는 경우
                SpawnMonsters(_currentWaveIndex);
                _currentWaveIndex++;
                
                // 트리거 박스 콜라이더를 비활성화 한다.
                DisableTrigger();
            }
        }

        private void DisableTrigger()
        {
            var col = GetComponent<BoxCollider>();
            if (col != null)
            {
                col.enabled = false;
            }
            else
            {
                Debug.LogError("스테이지에서 BoxCollider 컴포넌트를 찾을 수 없습니다.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // if (isInteract) return;    // 트리거 박스가 사용형태를 갖고 있다면, 
                                        // 플레이어가 스테이지에 들어왔을 때, 웨이브를 스폰하지 않는다.
                                        // 플레이어가 상호작용했을 때 스폰을 시작한다.
            if (!other.CompareTag("Player")) return;

            // _isPlayerInStage = true;
            // 플레이어가 스테이지에 들어왔을 때, 
            // 1. 현재 웨이브의 몬스터를 스폰한다.
            // 2. 트리거를 비활성화 시킨다.
            // 3. 웨이브 인덱스를 증가시킨다.
            
            if (_currentWaveIndex < waves.Count)
            {
                SpawnMonsters(_currentWaveIndex);
                _currentWaveIndex++;
                DisableTrigger();
            }
            else
            {
                Debug.LogWarning("현재 웨이브 인덱스가 웨이브 리스트의 범위를 초과했습니다.");
            }
        }

        private void Update()
        {
            if (!IsWaveAreCleared(_currentWaveIndex == 0 ? _currentWaveIndex : _currentWaveIndex - 1)) return;
            if (_currentWaveIndex >= waves.Count) return;
            SpawnMonsters(_currentWaveIndex);
            _currentWaveIndex++;
        }
        
        #endregion

        #region Private Methods

        private bool IsWaveAreCleared(int index)
        {
            if (index < 0 || index >= waves.Count)
            {
                Debug.LogError("Invalid wave index: " + index);
                return isStageCleared = false;
            }

            var wave = waves[index];
            if (wave is null)
            {
                Debug.LogError("Wave GameObject is null at index: " + index);
                return isStageCleared = false;
            }

            // Assuming WaveController has a method to check if the wave is cleared
            var waveController = wave.GetComponent<WaveController>();
            if (waveController is not null)
            {
                return isStageCleared = waveController.CheckWaveClear();
            }
            else
            {
                Debug.LogError("WaveController component not found on wave GameObject at index: " + index);
                return isStageCleared = false;
            }
        }
        
        private void SpawnMonsters(int index)
        {
            if (index < 0 || index >= waves.Count)
            {
                Debug.LogError("Invalid wave index: " + index);
                return;
            }

            var wave = waves[index];
            if (wave == null)
            {
                Debug.LogError("Wave GameObject is null at index: " + index);
                return;
            }

            // Assuming WaveController has a method to spawn monsters
            var waveController = wave.GetComponent<WaveController>();
            if (waveController != null)
            {
                waveController.SpawnMonsters();
            }
            else
            {
                Debug.LogError("WaveController component not found on wave GameObject at index: " + index);
            }
        }

        #endregion
        
        #region Public Methods

        public bool CheckStageClear()
        {
            // 스테이지의 클리어 여부를 확인
            // 스테이지의 클리어는 웨이브에 의존적
            // 모든 웨이브가 클리어 되었는지 확인 (클리어 되었다면 true, 아니면 false)
            
            if (waves.Count == 0)
            {
                Debug.LogWarning("No waves found in the stage.");
                return isStageCleared = true;
            }

            for (var i = 0; i < waves.Count; i++)
            {
                if (!IsWaveAreCleared(i))
                    return false;
            }

            return true;
        }

        public bool UseTrigger()
        {
            // 트리거 박스를 플레이어가 사용한다.

            return false;
        }
        
        #endregion
    }
}