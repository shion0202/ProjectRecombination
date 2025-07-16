using System.Collections.Generic;
using UnityEngine;

namespace Jaeho.DungeonScript
{
    public class DungeonManager : ManagerBase<DungeonManager>
    {
        [Header("Dungeon Settings")]
        [SerializeField] private DungeonState dungeonState = DungeonState.Loading;
        [SerializeField] private List<GameObject> stages = new List<GameObject>();
        // [SerializeField] private bool isDungeonCleared = false;
        
        [Header("Player Settings")]
        [SerializeField] private GameObject _playerStartPoint;
        [SerializeField] private GameObject _player;
        
        #region Getters and Setters

        public GameObject player => _player;
        
        #endregion

        #region Unity Methods

        private void Start()
        {
            InitDungeon();
        }

        private void Update()
        {
            switch (dungeonState)
            {
                case DungeonState.Loading:
                    // Initialize dungeon, load stages, and set up player
                    LoadDungeon();
                    break;
                case DungeonState.InProgress:
                    // Handle dungeon progression, player actions, and stage transitions
                    InProgress();
                    break;
                case DungeonState.Paused:
                    // Handle pause state, show UI, and wait for player input to resume
                    PauseDungeon();
                    break;
                case DungeonState.Rewarding:
                    // 보스가 죽거나 없어서 던전 클리어 보상 스탭으로 전환
                    RewardDungeon();
                    break;
            }
        }

        #endregion
        
        private void InitDungeon()
        {
            Debug.Log("InitDungeon");
            if (stages.Count == 0)
            {
                Debug.LogWarning("No stages found. Please add stages to the dungeon.");
            }
            else
            {
                // Initialize dungeon settings, load stages, and set up player
                if (_player == null)
                {
                    Debug.LogError("Player GameObject not found. Please ensure the player is tagged correctly.");
                    return;
                }
                if (_playerStartPoint == null)
                {
                    Debug.LogError("Player spawn point is not set. Please assign a spawn point in the inspector.");
                    return;
                }
                _player.transform.position = _playerStartPoint.transform.position;
            }
        }
        
        private void LoadDungeon()
        {
            Debug.Log("LoadDungeon");
            dungeonState = DungeonState.InProgress;
        }

        private void InProgress()
        {
            // Debug.Log("InProgress");
            if (stages.Count > 0 && !AllStageAreCleared())
            {
                // 보스 몬스터가 존재하는 동안 던전 진행 상태 유지
                dungeonState = DungeonState.InProgress;
            }
            else
            {
                // 보스 몬스터가 없으면 던전 클리어 상태로 전환
                dungeonState = DungeonState.Rewarding;
            }
        }
        
        private void PauseDungeon()
        {
            // Pause the dungeon, show UI, and wait for player input to resume
            dungeonState = DungeonState.Paused;
        }
        
        private void RewardDungeon()
        {
            // Handle dungeon rewards, show UI, and reset dungeon state
            
        }

        private bool AllStageAreCleared()
        {
            foreach (var stage in stages)
            {
                var isClear = stage.GetComponent<StageController>().CheckStageClear();
                if (!isClear)
                {
                    return false; // If any stage is not cleared, return false
                }
            }

            return true;
        }

        #region Editor Methods

        private GameObject NewStage(string stageName)
        {
            var obj = new GameObject(stageName);
            obj.AddComponent<StageController>();
            obj.AddComponent<BoxCollider>();
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            stages.Add(obj);
            
            var col = obj.GetComponent<BoxCollider>();
            col.isTrigger = true;
            return obj;
        }
        
        public void AddLastStage()
        {
            var obj = NewStage("Stage" + (stages.Count + 1));
            var stageController = obj.GetComponent<StageController>();
            stageController.stageType = StageType.Normal;
        }

        public void AddBossStage()
        {
            var obj = NewStage("BossStage");
            var stageController = obj.GetComponent<StageController>();
            stageController.stageType = StageType.Boss;
        }
        
        public void RemoveLastStageWithoutBossStage()
        {
            // 보스 스테이지를 제외한 마지막 스테이지 제거
            if (stages.Count <= 0)
            {
                Debug.LogWarning("No stages to remove.");
                return;
            }
            
            var lastStageIndex = stages.Count - 1;
            var lastStage = stages[lastStageIndex];

            if (lastStageIndex == 0 && lastStage.GetComponent<StageController>().stageType == StageType.Boss)
            {
                Debug.LogWarning("Cannot remove the last stage if it is a boss stage.");
            }
            else
            {
                while (lastStageIndex >= 0 && stages[lastStageIndex].GetComponent<StageController>().stageType == StageType.Boss)
                {
                    lastStageIndex--;
                }
                if (lastStageIndex < 0)
                {
                    Debug.LogWarning("No normal stage found to remove.");
                    return;
                }
                lastStage = stages[lastStageIndex];
                stages.RemoveAt(lastStageIndex);
                DestroyImmediate(lastStage);
            }
        }
        
        public void RemoveBossStage()
        {
            // 보스 스테이지 제거
            for (var i = 0; i < stages.Count; i++)
            {
                if (stages[i].GetComponent<StageController>().stageType != StageType.Boss) continue;
                DestroyImmediate(stages[i]);
                stages.RemoveAt(i);
                return;
            }
            Debug.LogWarning("No boss stage found to remove.");
        }

        #endregion
    }
}

