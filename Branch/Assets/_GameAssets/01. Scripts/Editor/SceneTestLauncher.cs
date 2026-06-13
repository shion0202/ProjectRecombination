using System.IO;
using System.Threading.Tasks;
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
    /// Addressables 에 등록되지 않은 씬을 위한 '직접 로드(OpenScene)' 폴백도 제공한다.
    /// </summary>
    public class SceneTestLauncher : EditorWindow
    {
        // StartBootstrap 과 동일한 부트스트랩 씬 경로. Addressables 로드 시 이 씬부터 시작한다.
        private const string BootstrapScenePath = "Assets/_GameAssets/04. Scenes/Bootstrap.unity";

        // 선택 내용을 프로젝트 단위로 보존하기 위한 EditorPrefs 키 (프로젝트명으로 스코프 분리).
        private static string TargetSceneKey => $"{PlayerSettings.productName}.SceneTestLauncher.TargetSceneGUID";
        private static string LoadModeKey => $"{PlayerSettings.productName}.SceneTestLauncher.UseAddressableLoad";

        // 플레이 진입(도메인 리로드)을 넘어 로드할 키를 전달하기 위한 SessionState 키.
        private const string PendingKeyState = "SceneTestLauncher.PendingAddressableKey";

        // 부트스트랩 초기화 대기 최대 프레임 수 (이 안에 SceneController 가 생성되지 않으면 강제로 진행).
        private const int MaxWaitFrames = 300;

        // EditorWindow 필드는 [SerializeField] 시 도메인 리로드(스크립트 컴파일) 후에도 유지된다.
        [SerializeField] private SceneAsset _targetScene;
        [SerializeField] private bool _useAddressableLoad = true;

        private static string _pendingKey;
        private static int _waitFrames;

        [MenuItem("Tools/Scene Test Launcher")]
        public static void Open()
        {
            SceneTestLauncher window = GetWindow<SceneTestLauncher>("Scene Test");
            window.minSize = new Vector2(340f, 170f);
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

            PlayScene(scene, EditorPrefs.GetBool(LoadModeKey, true));
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
            _useAddressableLoad = EditorPrefs.GetBool(LoadModeKey, true);
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
            _useAddressableLoad = EditorGUILayout.ToggleLeft(
                "Addressables 방식으로 로드 (부트스트랩 경유, 게임과 동일)", _useAddressableLoad);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(LoadModeKey, _useAddressableLoad);
            }

            // 선택한 씬의 Addressables 등록 여부를 안내.
            if (_useAddressableLoad && _targetScene != null && string.IsNullOrEmpty(GetAddressableKey(_targetScene)))
            {
                EditorGUILayout.HelpBox(
                    "선택한 씬이 Addressables 에 등록되어 있지 않습니다. Addressable 로 표시하거나 토글을 끄고 직접 로드하세요.",
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
                        PlayScene(_targetScene, _useAddressableLoad);
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

        /// <summary>
        /// 지정한 씬을 저장 절차를 거쳐 실행한다.
        /// Addressables 방식이면 부트스트랩부터 시작한 뒤 플레이 진입 후 SceneController 로 Additive 로드한다.
        /// 직접 방식이면 대상 씬을 바로 열고 플레이한다.
        /// </summary>
        private static void PlayScene(SceneAsset scene, bool useAddressableLoad)
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

            if (useAddressableLoad)
            {
                // 3-A. Addressables 키 확인 (게임이 사용하는 로드 키).
                string key = GetAddressableKey(scene);
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError($"[SceneTools] '{scenePath}' 가 Addressables 에 등록되어 있지 않습니다. " +
                                   "Addressable 로 표시하거나 직접 로드 방식을 사용하세요.");
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

                // 플레이 진입 시 도메인 리로드를 넘어 로드할 키를 전달.
                SessionState.SetString(PendingKeyState, key);
            }
            else
            {
                // 3-B. 직접 로드: 시작 씬 오버라이드 해제 후 대상 씬을 직접 연다.
                EditorSceneManager.playModeStartScene = null;
                SessionState.EraseString(PendingKeyState);
                EditorSceneManager.OpenScene(scenePath);
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
            if (change != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            // 도메인 리로드 이후 시점이므로 SessionState 에서 대기 중인 키를 읽는다.
            _pendingKey = SessionState.GetString(PendingKeyState, string.Empty);
            if (string.IsNullOrEmpty(_pendingKey))
            {
                return;
            }

            SessionState.EraseString(PendingKeyState);
            _waitFrames = 0;
            EditorApplication.update += LoadPendingSceneWhenReady;
        }

        // 부트스트랩 초기화로 SceneController 가 준비될 때까지 기다렸다가 Additive 로드한다.
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
            _pendingKey = null;
            _ = LoadAndActivate(key);
        }

        // 게임 런타임과 동일하게 Addressables Additive 로드 후 액티브 씬으로 전환.
        private static async Task LoadAndActivate(string key)
        {
            Debug.Log($"[SceneTools] Addressables 방식으로 테스트 씬 로드: {key}");
            await SceneController.Instance.LoadSceneAdditive(key);
            SceneController.Instance.SetActiveScene(key);
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
    }
}