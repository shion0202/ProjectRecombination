using System.IO;
using System.Threading.Tasks;
using Managers;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Project._01._Scripts.Editor
{
    /// <summary>
    /// 사용자가 지정한 씬을 빠르게 테스트(플레이)하기 위한 에디터 윈도우.
    /// 기본 동작은 게임 런타임과 동일하게, 부트스트랩에서 시작한 뒤 SceneController 를 통해
    /// 대상 씬을 Addressables 키 기반으로 Additive 로드한다. (SceneController.LoadSceneAdditive 참고)
    ///
    /// 로드 방식은 3가지를 제공한다.
    ///  - Direct        : Addressables 미등록 씬을 위한 직접 로드(OpenScene) 폴백.
    ///  - BootstrapAdditive : 부트스트랩 경유 후 대상 씬만 Additive 로드 (UI/자체 완결형 씬 테스트용).
    ///  - BootstrapInGame   : 부트스트랩 경유 후 GameManager.EnterPrologue() 의 인게임 셋업
    ///                        (Pool/Player/시작위치)을 재현하고 대상 씬을 로드 → 실제 플레이 가능한 상태.
    /// </summary>
    public class SceneTestLauncher : EditorWindow
    {
        // 씬 테스트 로드 방식.
        private enum SceneLoadMode
        {
            // 대상 씬을 에디터에서 직접 열고 플레이 (Addressables 미등록 씬 폴백).
            Direct = 0,
            // 부트스트랩 경유 후 대상 씬만 Additive 로드 (게임 로드 파이프라인 일부).
            BootstrapAdditive = 1,
            // 부트스트랩 경유 후 인게임 셋업(Pool/Player/시작위치)까지 재현하고 대상 씬 로드.
            BootstrapInGame = 2,
        }

        // StartBootstrap 과 동일한 부트스트랩 씬 경로. Addressables 로드 시 이 씬부터 시작한다.
        private const string BootstrapScenePath = "Assets/_GameAssets/04. Scenes/Bootstrap.unity";

        // 인게임 셋업 시 로드할 플레이어 씬 키 (GameManager.LoadPlayerScene 과 동일).
        private const string PlayerSceneKey = "Scene_Player";

        // 테스트 시작 지점으로 사용할 씬 내 게임오브젝트 이름.
        // 대상 씬에 이 이름의 오브젝트를 두면 플레이어가 해당 위치에서 테스트를 시작한다.
        private const string TestPlayerStartName = "TestPlayerStart";

        // 선택 내용을 프로젝트 단위로 보존하기 위한 EditorPrefs 키 (프로젝트명으로 스코프 분리).
        private static string TargetSceneKey => $"{PlayerSettings.productName}.SceneTestLauncher.TargetSceneGUID";
        private static string LoadModeKey => $"{PlayerSettings.productName}.SceneTestLauncher.LoadMode";
        private static string InvincibleKey => $"{PlayerSettings.productName}.SceneTestLauncher.Invincible";
        private static string DamageMultKey => $"{PlayerSettings.productName}.SceneTestLauncher.EnemyDamageMultiplier";

        // 플레이 진입(도메인 리로드)을 넘어 로드할 키/모드/옵션을 전달하기 위한 SessionState 키.
        private const string PendingKeyState = "SceneTestLauncher.PendingAddressableKey";
        private const string PendingModeState = "SceneTestLauncher.PendingLoadMode";
        private const string PendingInvincibleState = "SceneTestLauncher.PendingInvincible";
        private const string PendingMultiplierState = "SceneTestLauncher.PendingEnemyDamageMultiplier";

        // 적 데미지 배수 허용 범위 (1 = 기본, 영향 없음).
        private const float MinDamageMultiplier = 1f;
        private const float MaxDamageMultiplier = 1000f;

        // 부트스트랩 초기화 대기 최대 프레임 수 (이 안에 SceneController 가 생성되지 않으면 강제로 진행).
        private const int MaxWaitFrames = 300;

        // EditorWindow 필드는 [SerializeField] 시 도메인 리로드(스크립트 컴파일) 후에도 유지된다.
        [SerializeField] private SceneAsset _targetScene;
        [SerializeField] private SceneLoadMode _loadMode = SceneLoadMode.BootstrapInGame;
        [SerializeField] private bool _invincible;
        [SerializeField] private float _enemyDamageMultiplier = 1f;

        private static string _pendingKey;
        private static SceneLoadMode _pendingMode;
        private static bool _pendingInvincible;
        private static float _pendingMultiplier = 1f;
        private static int _waitFrames;

        [MenuItem("Tools/Scene Test Launcher")]
        public static void Open()
        {
            SceneTestLauncher window = GetWindow<SceneTestLauncher>("Scene Test");
            window.minSize = new Vector2(360f, 220f);
            window.Show();
        }

        // 단축키(F6)로 마지막에 지정한 테스트 씬을 바로 실행. 창을 열지 않아도 동작한다.
        // % = Ctrl/Cmd, # = Shift, & = Alt, _ = 보조키 없음.
        [MenuItem("Tools/Play Target Test Scene _F6")]
        public static void PlayTargetSceneShortcut()
        {
            // 플레이 중이면 정지 토글 (StartBootstrap 과 동일한 UX).
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            string scenePath = LoadSavedScenePath();
            SceneAsset scene = string.IsNullOrEmpty(scenePath)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (scene == null)
            {
                Debug.LogWarning("[SceneTools] 지정된 테스트 씬이 없습니다. 'Tools/Scene Test Launcher'에서 씬을 먼저 선택하세요.");
                return;
            }

            PlayScene(scene, LoadSavedMode(), LoadSavedInvincible(), LoadSavedMultiplier());
        }

        private void OnEnable()
        {
            // 저장된 선택을 복원.
            if (_targetScene == null)
            {
                string scenePath = LoadSavedScenePath();
                if (!string.IsNullOrEmpty(scenePath))
                {
                    _targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                }
            }
            _loadMode = LoadSavedMode();
            _invincible = LoadSavedInvincible();
            _enemyDamageMultiplier = LoadSavedMultiplier();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("테스트할 씬을 선택하고 실행합니다.", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 1. 테스트 씬 선택 (변경 시 EditorPrefs 에 즉시 저장).
            EditorGUI.BeginChangeCheck();
            _targetScene = (SceneAsset)EditorGUILayout.ObjectField("테스트 씬", _targetScene, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveScenePath(_targetScene);
            }

            // 2. 로드 방식 선택.
            EditorGUI.BeginChangeCheck();
            _loadMode = (SceneLoadMode)EditorGUILayout.EnumPopup("로드 방식", _loadMode);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(LoadModeKey, (int)_loadMode);
            }

            // 선택한 모드 설명.
            EditorGUILayout.HelpBox(DescribeMode(_loadMode), MessageType.None);

            // 2-1. 무적 모드 토글 (인게임 셋업 모드에서만 의미 있음 — 플레이어가 있어야 함).
            using (new EditorGUI.DisabledScope(_loadMode != SceneLoadMode.BootstrapInGame))
            {
                EditorGUI.BeginChangeCheck();
                _invincible = EditorGUILayout.ToggleLeft(
                    "무적 모드로 테스트 (플레이어 Invincibility 유지)", _invincible);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool(InvincibleKey, _invincible);
                }

                // 2-2. 적 데미지 배수 (플레이어 → 몬스터/오브젝트 데미지 증가, 1 = 기본).
                EditorGUI.BeginChangeCheck();
                _enemyDamageMultiplier = EditorGUILayout.FloatField(
                    "적 데미지 배수 (x)", _enemyDamageMultiplier);
                if (EditorGUI.EndChangeCheck())
                {
                    _enemyDamageMultiplier = Mathf.Clamp(_enemyDamageMultiplier, MinDamageMultiplier, MaxDamageMultiplier);
                    EditorPrefs.SetFloat(DamageMultKey, _enemyDamageMultiplier);
                }
            }

            // 선택한 씬의 Addressables 등록 여부를 안내 (부트스트랩 경유 모드에서만).
            bool needsAddressable = _loadMode != SceneLoadMode.Direct;
            if (needsAddressable && _targetScene != null && string.IsNullOrEmpty(GetAddressableKey(_targetScene)))
            {
                EditorGUILayout.HelpBox(
                    "선택한 씬이 Addressables 에 등록되어 있지 않습니다. Addressable 로 표시하거나 'Direct' 모드로 직접 로드하세요.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space();

            // 3. 실행 / 정지 버튼.
            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("정지 (Stop)", GUILayout.Height(32f)))
                {
                    EditorApplication.isPlaying = false;
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(_targetScene == null))
                {
                    if (GUILayout.Button("테스트 실행 (Play)", GUILayout.Height(32f)))
                    {
                        PlayScene(_targetScene, _loadMode, _invincible, _enemyDamageMultiplier);
                    }
                }

                if (_targetScene == null)
                {
                    EditorGUILayout.HelpBox("테스트할 씬을 먼저 지정하세요.", MessageType.Info);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("단축키: F6 (지정한 씬 실행/정지)", EditorStyles.miniLabel);
        }

        // 모드별 설명 문구.
        private static string DescribeMode(SceneLoadMode mode)
        {
            switch (mode)
            {
                case SceneLoadMode.Direct:
                    return "Direct: 대상 씬을 에디터에서 직접 열고 플레이합니다. (Addressables 미등록 씬용)";
                case SceneLoadMode.BootstrapAdditive:
                    return "BootstrapAdditive: 부트스트랩부터 시작해 대상 씬만 Additive 로드합니다. (UI/자체 완결형 씬)";
                case SceneLoadMode.BootstrapInGame:
                    return "BootstrapInGame: 부트스트랩 경유 후 Pool/Player 인게임 셋업을 재현하고 대상 씬을 로드합니다. " +
                           "플레이어는 씬 내 'TestPlayerStart' 오브젝트 위치에서 시작합니다. (던전/스테이지 씬을 실제 플레이 상태로 테스트)";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 지정한 씬을 저장 절차를 거쳐 실행한다.
        /// 부트스트랩 경유 모드면 부트스트랩부터 시작한 뒤 플레이 진입 후 SceneController 로 로드한다.
        /// 직접 방식이면 대상 씬을 바로 열고 플레이한다.
        /// </summary>
        private static void PlayScene(SceneAsset scene, SceneLoadMode mode, bool invincible, float enemyDamageMultiplier)
        {
            string scenePath = AssetDatabase.GetAssetPath(scene);

            // 1. 씬 파일 존재 확인.
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            {
                Debug.LogError($"[SceneTools] 테스트 씬을 찾을 수 없습니다. 경로를 확인하세요: {scenePath}");
                return;
            }

            // 2. 현재 작업 중인 씬 저장 (사용자가 취소하면 중단).
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (mode == SceneLoadMode.Direct)
            {
                // 3-A. 직접 로드: 시작 씬 오버라이드 해제 후 대상 씬을 직접 연다.
                EditorSceneManager.playModeStartScene = null;
                SessionState.EraseString(PendingKeyState);
                SessionState.EraseInt(PendingModeState);
                SessionState.EraseInt(PendingInvincibleState);
                SessionState.EraseFloat(PendingMultiplierState);
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                // 3-B. 부트스트랩 경유: Addressables 키 확인 (게임이 사용하는 로드 키).
                string key = GetAddressableKey(scene);
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError($"[SceneTools] '{scenePath}' 가 Addressables 에 등록되어 있지 않습니다. " +
                                   "Addressable 로 표시하거나 'Direct' 모드를 사용하세요.");
                    return;
                }

                // 부트스트랩부터 시작해 매니저/로딩 파이프라인을 게임과 동일하게 구성.
                SceneAsset bootstrap = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);
                if (bootstrap == null)
                {
                    Debug.LogError($"[SceneTools] 부트스트랩 씬을 찾을 수 없습니다: {BootstrapScenePath}");
                    return;
                }
                EditorSceneManager.playModeStartScene = bootstrap;

                // 플레이 진입 시 도메인 리로드를 넘어 로드할 키/모드/옵션을 전달.
                SessionState.SetString(PendingKeyState, key);
                SessionState.SetInt(PendingModeState, (int)mode);
                SessionState.SetInt(PendingInvincibleState, invincible ? 1 : 0);
                SessionState.SetFloat(PendingMultiplierState, enemyDamageMultiplier);
            }

            // 4. 플레이 모드 진입.
            EditorApplication.isPlaying = true;
        }

        // 플레이 모드 진입을 감지하기 위한 훅 등록 (에디터 로드/컴파일 시 1회).
        [InitializeOnLoadMethod]
        private static void RegisterPlayModeHook()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            // 플레이 종료 시 테스트용 적 데미지 배수를 기본값으로 되돌려 다음 실행에 누수되지 않게 한다.
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                TestManager.EnemyDamageMultiplier = 1f;
                return;
            }

            if (change != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            // 도메인 리로드 이후 시점이므로 SessionState 에서 대기 중인 키/모드/옵션을 읽는다.
            _pendingKey = SessionState.GetString(PendingKeyState, string.Empty);
            if (string.IsNullOrEmpty(_pendingKey))
            {
                return;
            }

            _pendingMode = (SceneLoadMode)SessionState.GetInt(PendingModeState, (int)SceneLoadMode.BootstrapAdditive);
            _pendingInvincible = SessionState.GetInt(PendingInvincibleState, 0) == 1;
            _pendingMultiplier = SessionState.GetFloat(PendingMultiplierState, 1f);

            SessionState.EraseString(PendingKeyState);
            SessionState.EraseInt(PendingModeState);
            SessionState.EraseInt(PendingInvincibleState);
            SessionState.EraseFloat(PendingMultiplierState);
            _waitFrames = 0;
            EditorApplication.update += LoadPendingSceneWhenReady;
        }

        // 부트스트랩 초기화로 SceneController 가 준비될 때까지 기다렸다가 로드한다.
        private static void LoadPendingSceneWhenReady()
        {
            // 플레이 모드를 벗어났으면 취소.
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= LoadPendingSceneWhenReady;
                _pendingKey = null;
                return;
            }

            // SceneController 생성 대기 (자동 생성을 강제하지 않도록 IsAliveInstance 로 확인).
            // 일정 프레임 안에 생성되지 않으면 강제로 진행한다(Instance 접근 시 자동 생성됨).
            _waitFrames++;
            if (!SceneController.IsAliveInstance() && _waitFrames < MaxWaitFrames)
            {
                return;
            }

            EditorApplication.update -= LoadPendingSceneWhenReady;

            string key = _pendingKey;
            SceneLoadMode mode = _pendingMode;
            _pendingKey = null;

            if (mode == SceneLoadMode.BootstrapInGame)
            {
                _ = LoadInGame(key);
            }
            else
            {
                _ = LoadAndActivate(key);
            }
        }

        // BootstrapAdditive: 게임 런타임과 동일하게 Addressables Additive 로드 후 액티브 씬으로 전환.
        private static async Task LoadAndActivate(string key)
        {
            Debug.Log($"[SceneTools] BootstrapAdditive 로 테스트 씬 로드: {key}");
            await SceneController.Instance.LoadSceneAdditive(key);
            SceneController.Instance.SetActiveScene(key);
        }

        // BootstrapInGame: GameManager.EnterPrologue() 의 인게임 셋업(Pool/Player/시작위치)을 재현하되
        // 기본 스테이지 대신 대상 씬을 로드하여, 던전/스테이지 씬을 실제 플레이 가능한 상태로 만든다.
        private static async Task LoadInGame(string key)
        {
            Debug.Log($"[SceneTools] BootstrapInGame 셋업 시작: {key}");

            // 0. 테스트용 적 데미지 배수 적용 (FSM.OnHit / DamagableObject.ApplyDamage 가 참조).
            TestManager.EnemyDamageMultiplier = _pendingMultiplier;
            if (!Mathf.Approximately(_pendingMultiplier, 1f))
            {
                Debug.Log($"[SceneTools] 적 데미지 배수 적용: x{_pendingMultiplier}");
            }

            // 1. 몬스터 풀 초기화 (대상 씬의 몬스터가 풀을 참조하므로 가장 먼저).
            await PoolManager.Instance.Init();

            // 2. 플레이어 씬 로드 (InitPlayer 가 GameManager.Player / 카메라 등을 등록).
            await SceneController.Instance.LoadSceneAdditive(PlayerSceneKey);

            // 3. 대상 스테이지 씬 로드.
            await SceneController.Instance.LoadSceneAdditive(key);

            // 4. 대상 씬을 액티브 씬으로 전환.
            SceneController.Instance.SetActiveScene(key);

            // 5. 플레이어를 씬 내 'TestPlayerStart' 지점으로 이동.
            MovePlayerToTestStart();

            // 6. 프롤로그를 건너뛰고 바로 플레이 상태로 진입.
            GameManager.Instance.StartGame();

            // 7. 씬에 배치된 테스트 훅 실행 (예: 아몬 2페이즈 FSM 직접 활성화).
            //    실제 게임에서 선행 조건으로만 켜지는 로직을 선행 조건 없이 바로 동작시킨다.
            InvokeSceneTestHooks();

            // 8. 무적 모드 토글이 켜져 있으면 플레이어에 무적 유지 컴포넌트를 부착.
            if (_pendingInvincible)
            {
                EnableInvincibility();
            }

            Debug.Log($"[SceneTools] BootstrapInGame 셋업 완료: {key}");
        }

        // 플레이어에 SceneTestInvincibility 를 부착해 테스트 내내 무적 상태를 유지한다.
        private static void EnableInvincibility()
        {
            PlayerController player = GameManager.Instance.Player;
            if (player == null)
            {
                Debug.LogWarning("[SceneTools] 플레이어가 없어 무적 모드를 적용하지 못했습니다.");
                return;
            }

            if (player.GetComponent<SceneTestInvincibility>() == null)
            {
                player.gameObject.AddComponent<SceneTestInvincibility>();
            }

            Debug.Log("[SceneTools] 무적 모드 활성화 (플레이어 Invincibility 유지).");
        }

        // 현재 로드된 씬들에서 ISceneTestHook 구현 컴포넌트를 찾아 OnTestStart 를 호출한다.
        // (비활성 오브젝트 포함. 보통 대상 테스트 씬에만 존재한다.)
        private static void InvokeSceneTestHooks()
        {
            MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is ISceneTestHook hook)
                {
                    hook.OnTestStart();
                }
            }
        }

        // 대상 씬에 배치된 'TestPlayerStart' 오브젝트 위치로 플레이어를 이동시킨다.
        // CharacterController 가 직접 위치 설정을 무시하므로 잠시 비활성화 후 적용한다.
        // (DungeonManager.ResetCurrentStage / AmonSecondPhase 의 위치 강제 이동 방식과 동일.)
        private static void MovePlayerToTestStart()
        {
            PlayerController player = GameManager.Instance.Player;
            if (player == null)
            {
                Debug.LogWarning("[SceneTools] 플레이어가 없어 시작 위치 이동을 건너뜁니다.");
                return;
            }

            GameObject startObj = GameObject.Find(TestPlayerStartName);
            if (startObj == null)
            {
                Debug.LogWarning($"[SceneTools] 씬에서 '{TestPlayerStartName}' 오브젝트를 찾지 못했습니다. " +
                                 "플레이어가 기본 위치에서 시작합니다.");
                return;
            }

            Transform playerTransform = player.transform;
            CharacterController controller = player.GetComponent<CharacterController>();

            if (controller != null) controller.enabled = false;
            playerTransform.SetPositionAndRotation(startObj.transform.position, startObj.transform.rotation);
            if (controller != null) controller.enabled = true;

            Debug.Log($"[SceneTools] 플레이어를 '{TestPlayerStartName}' 위치로 이동: {startObj.transform.position}");
        }

        // SceneAsset 의 Addressables 주소(키)를 조회. 등록돼 있지 않으면 빈 문자열.
        private static string GetAddressableKey(SceneAsset scene)
        {
            if (scene == null)
            {
                return string.Empty;
            }

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scene));
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return string.Empty;
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            return entry != null ? entry.address : string.Empty;
        }

        // 선택한 씬의 GUID 를 EditorPrefs 에 저장.
        private static void SaveScenePath(SceneAsset scene)
        {
            if (scene == null)
            {
                EditorPrefs.DeleteKey(TargetSceneKey);
                return;
            }
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scene));
            EditorPrefs.SetString(TargetSceneKey, guid);
        }

        // 저장된 GUID 로부터 씬 경로를 복원. 없으면 빈 문자열.
        private static string LoadSavedScenePath()
        {
            string guid = EditorPrefs.GetString(TargetSceneKey, string.Empty);
            if (string.IsNullOrEmpty(guid))
            {
                return string.Empty;
            }
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        // 저장된 로드 방식을 복원. 없으면 인게임 셋업 모드.
        private static SceneLoadMode LoadSavedMode()
        {
            return (SceneLoadMode)EditorPrefs.GetInt(LoadModeKey, (int)SceneLoadMode.BootstrapInGame);
        }

        // 저장된 무적 모드 토글을 복원. 없으면 끔.
        private static bool LoadSavedInvincible()
        {
            return EditorPrefs.GetBool(InvincibleKey, false);
        }

        // 저장된 적 데미지 배수를 복원. 없으면 1 (영향 없음).
        private static float LoadSavedMultiplier()
        {
            return Mathf.Clamp(EditorPrefs.GetFloat(DamageMultKey, 1f), MinDamageMultiplier, MaxDamageMultiplier);
        }
    }
}