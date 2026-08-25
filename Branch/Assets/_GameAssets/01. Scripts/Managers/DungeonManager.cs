using Monster.AI.FSM;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Managers
{
    [Serializable]
    public struct StageData
    {
        public int stageIndex;
        public string stageName;
    }
    
    public class DungeonManager : Singleton<DungeonManager>
    {
        [SerializeField] private GameObject miniMapPrefab;
        [SerializeField] private GameObject startPosition;
        
        // 스테이지 데이터
        [SerializeField] private StageData[] stageDatas;

        // 체험 플레이(Demo) 전용 스테이지 데이터. 원소 1개(튜토리얼 씬)만 사용한다.
        [SerializeField] private StageData[] demoStageDatas;

        // 현재 플레이 모드에 해당하는 스테이지 세트.
        // Init / UnloadAllStage / ResetCurrentStage / UpdatePlayerStageIndex 는 모두 이 프로퍼티를 참조한다.
        private StageData[] CurrentStageSet =>
            GameManager.Instance.PlayMode == EPlayMode.Demo ? demoStageDatas : stageDatas;

        // 현재 플레이어가 있는 스테이지 인덱스
        [SerializeField] private int currentPlayerStageIndex;
        public int CurrentPlayerStageIndex { get => currentPlayerStageIndex; private set => currentPlayerStageIndex = value; }
        
        public Vector3 RestartPosition { get; set; }
        
        // 로딩된 스테이지 딕셔너리
        private Dictionary<int, string> LoadedStages { get; set; } = new();

        // 스테이지 갱신 중 여부
        private bool _isUpdatingStage = false;

        // LoadMiniMap()이 생성한 미니맵 인스턴스. Persistent 씬에 남으므로 세션 종료 시 직접 파괴한다.
        private GameObject _miniMapInstance;

        #region Initialization

        private bool _isInit;
        
        public async Task Init()
        {
            try
            {
                if (_isInit) return;
                
                // 초기화 작업 수행
                CurrentPlayerStageIndex = 0;
                LoadedStages.Clear();

                // 스테이지 데이터 로드 (플레이어의 현재 위치 + 주변 스테이지 로딩
                foreach (StageData stageData in CurrentStageSet)
                {
                    if (stageData.stageIndex != CurrentPlayerStageIndex + 1 &&
                        stageData.stageIndex != CurrentPlayerStageIndex) continue;

                    await LoadStage(stageData);
                }
                
                // 미니맵 로드
                LoadMiniMap();
                
                _isInit = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DungeonManager] 초기화 중 예외 발생: {e}");
            }
        }

        /// <summary>
        /// 한 판이 끝났을 때 스테이지 로드 상태를 초기 상태로 되돌린다.
        ///
        /// _isInit이 복구되지 않으면 2회차 Init()이 맨 앞에서 return하여 스테이지가 아예 로드되지 않고,
        /// LoadedStages가 비워지지 않으면 LoadStage()가 "이미 로드됨"으로 판단해 스킵한다.
        /// </summary>
        public void ResetSession()
        {
            _isInit = false;
            LoadedStages.Clear();
            CurrentPlayerStageIndex = 0;

            // 미니맵은 Persistent 씬에 생성되어 씬 언로드로 정리되지 않는다.
            // 여기서 파괴하지 않으면 _isInit 복구와 맞물려 판마다 하나씩 쌓인다.
            if (_miniMapInstance != null)
            {
                Destroy(_miniMapInstance);
                _miniMapInstance = null;
            }

            // 아래 참조들은 BossTrigger(본편) 또는 TutorialDirector(데모)가 "씬에서" 주입한다.
            // 씬이 언로드되면 파괴된 오브젝트를 가리키게 되므로 반드시 해제한다.
            amonFirstPhase = null;
            amonSecondPhasePrefab = null;
            playerTeleportPoint = null;
            playerRespawnPoint = null;
            startPosition = null;

            Debug.Log("[DungeonManager] 세션 리셋 완료");
        }

        public void SetStartPosition(GameObject obj)
        {
            startPosition = obj;
        } 

        public void SetPlayerStartPosition()
        {
            try
            {
                if (GameManager.Instance.Player is null) return;

                PlayerController player = GameManager.Instance.Player;
                CharacterController controller = player.GetComponent<CharacterController>();

                if (startPosition != null)
                {
                    // CharacterController가 켜져 있으면 transform 직접 대입이 되돌려질 수 있으므로
                    // 껐다 옮긴다. (AmonSecondPhase / AmonEndPhase 와 동일한 패턴)
                    if (controller != null) controller.enabled = false;
                    player.transform.position = startPosition.transform.position;
                }
                else
                {
                    Debug.LogWarning("[DungeonManager] 시작 위치가 지정되지 않아 플레이어를 옮기지 않습니다.");
                }

                // LoadPlayerScene이 낙하 방지를 위해 꺼둔 컨트롤러를 여기서 반드시 다시 켠다.
                // 시작 위치가 없더라도 켜지 않으면 플레이어가 영구히 움직이지 못하므로
                // 위 분기와 무관하게 실행한다.
                if (controller != null) controller.enabled = true;

                // 지형이 없는 동안 누적됐을 낙하 속도를 지운다. 남아 있으면 배치 직후 바닥을 뚫는다.
                player.ResetGravityAndFalling();

                // Dynamic 씬을 Active 씬으로 설정
                //SceneController.Instance.SetActiveScene(LoadedStages[CurrentPlayerStageIndex]);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DungeonManager] 플레이어 시작 위치 설정 중 예외 발생: {e}");
            }
        }

        #endregion

        #region Manage Stage Load/Unload

        private async Task LoadStage(StageData stageData)
        {
            if (LoadedStages.ContainsKey(stageData.stageIndex)) return;

            LoadedStages.Add(stageData.stageIndex, stageData.stageName);

            await SceneController.Instance.LoadSceneAdditive(stageData.stageName);
        }
        
        private async Task UnloadStage(StageData stageData)
        {
            if (!LoadedStages.ContainsKey(stageData.stageIndex)) return;

            LoadedStages.Remove(stageData.stageIndex);

            await SceneController.Instance.UnloadScene(stageData.stageName);
        }
        
        public async void UpdatePlayerStageIndex(int newStageIndex)
        {
            // 이미 스테이지 갱신 작업 중이라면 추가 요청 무시
            if (_isUpdatingStage) return;

            try
            {
                if (newStageIndex == CurrentPlayerStageIndex) return;
                _isUpdatingStage = true;

                // 새로운 스테이지 인덱스에 따라 필요한 스테이지 로드/언로드
                {
                    // newStageIndex 값이 현재 플레이어 스테이지 인덱스보다 작아지는 경우 (뒤로 이동)
                    if (newStageIndex < CurrentPlayerStageIndex)
                    {
                        if (LoadedStages.ContainsKey(CurrentPlayerStageIndex + 1))
                            await UnloadStage(CurrentStageSet[CurrentPlayerStageIndex + 1]);

                        await LoadStage(CurrentStageSet[newStageIndex - 1 < 0 ? 0 : newStageIndex - 1]);
                    }
                    // newStageIndex 값이 현재 플레이어 스테이지 인덱스보다 커지는 경우 (앞으로 이동)
                    else if (newStageIndex > CurrentPlayerStageIndex)
                    {
                        if (LoadedStages.ContainsKey(CurrentPlayerStageIndex - 1))
                            await UnloadStage(CurrentStageSet[CurrentPlayerStageIndex - 1]);

                        await LoadStage(CurrentStageSet[newStageIndex + 1]);
                    }
                }
                
                CurrentPlayerStageIndex = newStageIndex;
                
                // 현재 플레이어가 있는 스테이지의 Dynamic 씬을 Active 씬으로 설정
                //SceneController.Instance.SetActiveScene(LoadedStages[CurrentPlayerStageIndex]);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DungeonManager] 플레이어 스테이지 인덱스 업데이트 중 예외 발생: {e}");
            }
            finally
            {
                _isUpdatingStage = false;
            }
        }

        #endregion

        #region Test Load All Stages

        /// <summary>
        /// 테스트용 모든 스테이지 로드
        /// </summary>
        public async Task LoadAllStage()
        {
            try
            {
                Debug.Log("[DungeonManager] 모든 스테이지 로드 시작...");
                
                foreach (StageData stageData in stageDatas)
                {
                    await SceneController.Instance.LoadSceneAdditive(stageData.stageName + "/Static");
                    await SceneController.Instance.LoadSceneAdditive(stageData.stageName + "/Dynamic");
                    await SceneController.Instance.LoadSceneAdditive(stageData.stageName + "/Hybrid");
                }
                
                LoadMiniMap();
                
                Debug.Log("[DungeonManager] 모든 스테이지 로드 완료!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DungeonManager] 모든 스테이지 로드 중 예외 발생: {e}");
            }
        }

        #endregion
        
        /// <summary>
        /// 모든 스테이지 언로드
        /// </summary>
        public async Task UnloadAllStage()
        {
            try
            {
                Debug.Log("[DungeonManager] 모든 스테이지 언로드 시작...");

                foreach (StageData stageData in CurrentStageSet)
                {
                    await SceneController.Instance.UnloadScene(stageData.stageName);
                }
                
                Debug.Log("[DungeonManager] 모든 스테이지 언로드 완료!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DungeonManager] 모든 스테이지 언로드 중 예외 발생: {e}");
            }
        }

        /// <summary>
        /// 미니맵 리소스 로드
        /// </summary>
        private void LoadMiniMap()
        {
            if (miniMapPrefab != null)
            {
                // 이 Instantiate는 씬 로드 중이 아니라 Init() 흐름에서 실행되므로
                // Active Scene(Scene_Persistent)에 생성된다. 즉 스테이지/플레이어 씬을 언로드해도
                // 살아남아 판마다 누적되므로, 인스턴스를 들고 있다가 ResetSession()에서 직접 파괴한다.
                _miniMapInstance = Instantiate(miniMapPrefab);
            }
            else
            {
                Debug.LogWarning("[DungeonManager] 미니맵 프리팹이 할당되지 않았습니다.");
            }
        }
        
        #region 아몬 페이즈 설정

        [Header("아몬 1페이즈 설정")]
        [SerializeField] private FSM amonFirstPhase;
        
        [Header("아몬 2페이즈 설정")]
        [SerializeField] private FSM amonSecondPhasePrefab;
        // [SerializeField] private Transform amonSpawnPoint;
        [SerializeField] private Transform playerTeleportPoint;
        [SerializeField] private Transform playerRespawnPoint;

        public FSM AmonFirstPhasePrefab { set { amonFirstPhase = value; } }
        public FSM AmonSecondPhasePrefab { set { amonSecondPhasePrefab = value; } }
        public Transform PlayerTeleportPoint { set { playerTeleportPoint = value; } }
        public Transform PlayerRespawnPoint { set { playerRespawnPoint = value; } }

        #endregion

        #region 아몬 페이즈 관리

        public void AmonFirstPhase()
        {
            amonFirstPhase.isEnabled = true;
        }
        
        // 아몬 1페이즈 종료 및 2페이즈 시작
        public void AmonSecondPhase()
        {
            // amonSecondPhasePrefab.SetActive(true);
            amonFirstPhase.isEnabled = false;
            // playerRespawnPoint.position = MonsterManager.Instance.Player.transform.position;
            //MonsterManager.Instance.Player.SetActive(false);
            //MonsterManager.Instance.Player.transform.position = playerTeleportPoint.position;
            //MonsterManager.Instance.Player.SetActive(true);

            // Player를 끄지 말고 이동에 방해되는 컴포넌트만 제어
            GameObject playerObj = MonsterManager.Instance.Player;
            CharacterController controller = playerObj.GetComponent<CharacterController>();
            // 위치 강제 수정을 위해 컨트롤러를 비활성화
            if (controller != null) controller.enabled = false;
            playerObj.transform.position = playerTeleportPoint.position;
            if (controller != null) controller.enabled = true;

            amonSecondPhasePrefab.isEnabled = true;
        }
        
        // 아몬 2페이즈 종료
        public void AmonEndPhase()
        {
            if (amonSecondPhasePrefab != null)
                amonSecondPhasePrefab.isEnabled = false;

            // 체험 플레이에서는 리스폰이 아니라 체험 종료로 이어진다.
            if (GameManager.Instance.PlayMode == EPlayMode.Demo)
            {
                // TODO(Task 6): 결과 화면 EnterDemoResult() 로 교체
                GameManager.Instance.ReturnToTitleFromDemo();
                return;
            }

            // 씬에서 주입되는 참조들이라 세션 리셋 후에는 비어 있을 수 있다.
            GameObject playerObj = MonsterManager.Instance.Player;
            if (playerObj == null || playerRespawnPoint == null) return;

            CharacterController controller = playerObj.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            playerObj.transform.position = playerRespawnPoint.position;
            if (controller != null) controller.enabled = true;
        }

        #endregion

        public async void ResetCurrentStage()
        {
            try
            {
                Debug.Log("[DungeonManager] 현재 스테이지 리셋 시작...");
                
                if (!LoadedStages.ContainsKey(CurrentPlayerStageIndex))
                {
                    Debug.LogWarning("[DungeonManager] 현재 스테이지가 로드되어 있지 않습니다.");
                    return;
                }
                
                StageData currentStageData = CurrentStageSet[CurrentPlayerStageIndex];
                
                // 1. 현재 스테이지 언로드
                await SceneController.Instance.UnloadScene(currentStageData.stageName);
                
                // 2. 현재 스테이지 다시 로드
                await SceneController.Instance.LoadSceneAdditive(currentStageData.stageName);
                
                // 3. 플레이어 위치 리스폰 지점으로 이동
                if (GameManager.Instance.Player)
                {
                    GameManager.Instance.Player.enabled = false;
                    GameManager.Instance.Player.transform.position = RestartPosition;
                    GameManager.Instance.Player.enabled = true;
                }

                currentPlayerStageIndex--;
                //SceneController.Instance.SetActiveScene(LoadedStages[CurrentPlayerStageIndex]);
                
                Debug.Log("[DungeonManager] 현재 스테이지 리셋 완료!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DungeonManager] 현재 스테이지 리셋 중 예외 발생: {e}");
            }
        }
    }
}
