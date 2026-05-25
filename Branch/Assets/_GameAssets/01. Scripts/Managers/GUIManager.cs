using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Managers
{
    public class GUIManager : Singleton<GUIManager>
    {
        private GameObject TitleUI { get; set; }
        private GameObject PrologueUI { get; set; }
        public GameUIController GameUIController { get; private set; }
        private GameObject EpilogueUI { get; set; }
        private GameObject LoadingUI { get; set; }
        private GameObject CreditUI { get; set; }

        private bool _isInit;

        [SerializeField] private AudioMixer audioMixer;

        public void Init(Dictionary<EUIType, GameObject> uiInstances)
        {
            try
            {
                if (uiInstances.TryGetValue(EUIType.Title, out GameObject titleUI) && titleUI != null)
                {
                    TitleUI = titleUI;
                }
                if (uiInstances.TryGetValue(EUIType.Prologue, out GameObject prologueUI) && prologueUI != null)
                {
                    PrologueUI = prologueUI;
                }
                if (uiInstances.TryGetValue(EUIType.GameUIController, out GameObject gameUIController) &&
                    gameUIController != null)
                {
                    GameUIController = gameUIController.GetComponent<GameUIController>();
                }
                if (uiInstances.TryGetValue(EUIType.Epilogue, out GameObject epilogueUI) && epilogueUI != null)
                {
                    EpilogueUI = epilogueUI;
                }
                if (uiInstances.TryGetValue(EUIType.Loading, out GameObject loadingUI) && loadingUI != null)
                {
                    LoadingUI = loadingUI;
                }
                if (uiInstances.TryGetValue(EUIType.Credit, out GameObject creditUI) && creditUI != null)
                {
                    CreditUI = creditUI;
                }
                
                CheckValidation(EUIType.Title, titleUI);
                CheckValidation(EUIType.Prologue, prologueUI);
                CheckValidation(EUIType.GameUIController, gameUIController);
                CheckValidation(EUIType.Epilogue, epilogueUI);
                CheckValidation(EUIType.Loading, loadingUI);
                CheckValidation(EUIType.Credit, creditUI);

                _isInit = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            AutoDetectHardwarePerformance();
            SetVolumeOptions();
        }
        
        private static void CheckValidation(EUIType uiType, GameObject uiInstance)
        {
            if (uiInstance == null)
            {
                Debug.LogError($"[GUIManager] {uiType} UI instance is null.");
            }
        }

        private void Update()
        {
            if (!_isInit) return;
            
            switch (GameManager.Instance.CurrentState)
            {
                case GameManager.GameState.Title:
                    TitleUI.SetActive(true);
                    PrologueUI.SetActive(false);
                    GameUIController.gameObject.SetActive(false);
                    EpilogueUI.SetActive(false);
                    LoadingUI.SetActive(false);
                    CreditUI.SetActive(false);
                    break;
                case GameManager.GameState.Prologue:
                    TitleUI.SetActive(false);
                    PrologueUI.SetActive(true);
                    GameUIController.gameObject.SetActive(false);
                    EpilogueUI.SetActive(false);
                    LoadingUI.SetActive(false);
                    CreditUI.SetActive(false);
                    break;
                case GameManager.GameState.Playing:
                    TitleUI.SetActive(false);
                    PrologueUI.SetActive(false);
                    GameUIController.gameObject.SetActive(true);
                    EpilogueUI.SetActive(false);
                    LoadingUI.SetActive(false);
                    CreditUI.SetActive(false);
                    break;
                case GameManager.GameState.Epilogue:
                    TitleUI.SetActive(false);
                    PrologueUI.SetActive(false);
                    GameUIController.gameObject.SetActive(false);
                    EpilogueUI.SetActive(true);
                    LoadingUI.SetActive(false);
                    CreditUI.SetActive(false);
                    break;
                case GameManager.GameState.Loading:
                    TitleUI.SetActive(false);
                    PrologueUI.SetActive(false);
                    GameUIController.gameObject.SetActive(false);
                    EpilogueUI.SetActive(false);
                    LoadingUI.SetActive(true);
                    CreditUI.SetActive(false);
                    break;
                case GameManager.GameState.Credit:
                    TitleUI.SetActive(false);
                    PrologueUI.SetActive(false);
                    GameUIController.gameObject.SetActive(false);
                    EpilogueUI.SetActive(false);
                    LoadingUI.SetActive(false);
                    CreditUI.SetActive(true);
                    break;
            }
        }

        private void AutoDetectHardwarePerformance()
        {
            bool isHDREnabled = false;

            // 이미 사용자가 설정을 변경한 적이 있는지 확인 (저장된 값이 있으면 자동 설정 건너뜀)
            if (PlayerPrefs.HasKey("HDREnabled"))
            {
                isHDREnabled = PlayerPrefs.GetInt("HDREnabled") == 1;
                SetHDR(isHDREnabled);
                return;
            }

            // 하드웨어 정보 읽기
            int vram = SystemInfo.graphicsMemorySize; // GPU 메모리 (MB 단위)
            int cpuCount = SystemInfo.processorCount; // CPU 코어 수

            // 고사양 기준 설정
#if UNITY_EDITOR || UNITY_STANDALONE
            // PC 기준: 1660Ti 기준 6GB 모델이 존재하며, 1060은 3GB 모델도 존재
            if (vram > 3000 && cpuCount >= 6)
            {
                isHDREnabled = true;
            }
            else
            {
                isHDREnabled = false; // 저사양으로 간주하여 HDR 비활성화
            }
#else
            // 모바일 기준: 기기 특성상 Vram 인식 수치가 유동적이므로, CPU 코어 수를 더 중점적으로 판단
            if (vram > 1500 && cpuCount >= 8)
            {
                isHDREnabled = true;
            }
            else
            {
                isHDREnabled = false; // 저사양으로 간주하여 HDR 비활성화
            }
#endif

            // 결정된 기본값 저장
            PlayerPrefs.SetInt("HDREnabled", isHDREnabled ? 1 : 0);
            SetHDR(isHDREnabled);
        }

        public void SetHDR(bool enable)
        {
            // 현재 사용 중인 Render Pipeline Asset을 가져옵니다.
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                // HDR 설정을 변경합니다.
                urpAsset.supportsHDR = enable;
            }
            PlayerPrefs.SetInt("HDREnabled", enable ? 1 : 0);
        }

        public void SetVolumeOptions()
        {
            float volume = PlayerPrefs.GetFloat("BGMParam", 0.8f);
            volume = Mathf.Clamp(volume, 0.0001f, 1.0f);
            audioMixer.SetFloat("BGMParam", Mathf.Log10(volume) * 20);

            volume = PlayerPrefs.GetFloat("SEParam", 0.8f);
            volume = Mathf.Clamp(volume, 0.0001f, 1.0f);
            audioMixer.SetFloat("SEParam", Mathf.Log10(volume) * 20);
        }

        /// <summary>
        /// GUI Load
        /// </summary>
        public static async Task LoadGUI()
        {
            try
            {
                Debug.Log("[GUIManager] GUI 매니저 초기화 시작...");
                await SceneController.Instance.LoadSceneAdditive("Scene_UI");
                Debug.Log("[GUIManager] GUI 매니저 초기화 완료!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GUIManager] GUI 매니저 초기화 중 예외 발생: {e}");
            }
        }
        
        /// <summary>
        /// GUI Unload
        /// </summary>
        public async Task UnloadGUI()
        {
            try
            {
                if (!_isInit) return;
                
                Debug.Log("[GUIManager] GUI 언로드 시작...");
                
                _isInit = false;
                
                // 모든 트윈 애니메이션 종료
                DOTween.KillAll();
                
                TitleUI = null;
                PrologueUI = null;
                GameUIController = null;
                EpilogueUI = null;
                LoadingUI = null;
                CreditUI = null;
                
                await SceneController.Instance.UnloadScene("Scene_UI");
                
                Debug.Log("[GUIManager] GUI 언로드 완료!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GUIManager] GUI 언로드 중 예외 발생: {e}");
            }
        }
    }
}