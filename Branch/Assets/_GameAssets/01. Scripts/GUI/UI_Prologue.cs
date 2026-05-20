using Managers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public struct VideoSpeaker
{
    public RawImage imageSceneA;            // 영상 재생 UI
    public RawImage imageSceneB;            // 두 번째 영상 재생 UI
    public TextMeshProUGUI textDialog;      // 텍스트 스크립트 출력 UI
    public GameObject objectArrow;          // 스크립트 표시 완료 후 활성화할 커서 오브젝트
    public Image fadePanel;                 // 페이드 인/아웃용 패널
}

[System.Serializable]
public struct DialogVideoData
{
    public string videoName;
    [TextArea(3, 5)] public string dialog;  // 텍스트 스크립트
}

public class UI_Prologue : MonoBehaviour, PlayerActions.IUIActionMapActions
{
    [Header("배경 이미지 및 스크립트 설정")]
    [SerializeField] private VideoSpeaker speaker;
    [SerializeField] private DialogVideoData[] dialogs;
    [SerializeField] private VideoPlayer videoPlayerA;
    [SerializeField] private VideoPlayer videoPlayerB;
    [SerializeField] private string nextSceneName = "GameScene";
    [SerializeField] private bool isAutoStart = true;
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float fadeDuration = 0.5f;

    private int _currentDialogIndex = -1;
    private bool _isTypingEffect;
    private bool _isTransitioning; // 암전 패널이 움직이거나 강제 대기 중일 때 입력 방지 플래그
    private bool _isFadeOut;
    private bool _isFadeIn;
    private bool _isEnd;
    private Coroutine _nextRoutine;
    private Coroutine _fadeRoutine;
    private Coroutine _typingRoutine;

    private PlayerActions _uiActions;
    private bool _isUpdateDialogue;

    // 현재 상영 중인 플레이어와 다음에 상영될 플레이어를 가리키는 포인터 필드
    private VideoPlayer _currentVideoPlayer;
    private VideoPlayer _nextVideoPlayer;
    private RawImage _currentRawImage;
    private RawImage _nextRawImage;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        if (isAutoStart)
        {
            SetNextDialog();
        }
    }

    private void Update()
    {
        if (_isEnd) return;

        if (_isUpdateDialogue)
        {
            if (!nextSceneName.Equals("GameScene"))
            {
                _uiActions.UIActionMap.Disable();
                GameManager.Instance.EnterCredit();
                return;
            }

            StartCoroutine(FadePanelAndGoToNext(speaker.fadePanel, 0f, 1f, fadeDuration));
            _isUpdateDialogue = false;
        }
    }

    private void OnEnable() => _uiActions?.UIActionMap.Enable();
    private void OnDisable() => _uiActions?.UIActionMap.Disable();
    private void OnDestroy()
    {
        _uiActions?.UIActionMap.Disable();
        _uiActions?.Dispose();
    }

    private void Init()
    {
        if (_uiActions != null)
        {
            _uiActions.UIActionMap.Disable();
            _uiActions.Dispose();
        }
        _uiActions = new PlayerActions();
        _uiActions.UIActionMap.SetCallbacks(this);
        _uiActions.UIActionMap.Enable();

        speaker.textDialog.text = "";
        speaker.objectArrow.SetActive(false);

        // 플레이어 초기 핑퐁 셋업 포인터 연결
        _currentVideoPlayer = videoPlayerA;
        _nextVideoPlayer = videoPlayerB;
        _currentRawImage = speaker.imageSceneA;
        _nextRawImage = speaker.imageSceneB;

        ConfigureVideoPlayer(videoPlayerA);
        ConfigureVideoPlayer(videoPlayerB);

        // 두 영상 캔버스를 모두 켜두되, 처음에는 암전 패널이 덮고 있도록 설정
        _currentRawImage.gameObject.SetActive(true);
        _nextRawImage.gameObject.SetActive(true);
        _currentRawImage.color = Color.white;
        _nextRawImage.color = Color.white;

        if (speaker.fadePanel != null)
        {
            speaker.fadePanel.gameObject.SetActive(true);
            speaker.fadePanel.color = new Color(0, 0, 0, 1f); // 시작은 까맣게
        }
    }

    private void ConfigureVideoPlayer(VideoPlayer vp)
    {
        vp.playOnAwake = false;
        vp.source = VideoSource.Url;
        vp.waitForFirstFrame = true;
        vp.skipOnDrop = true;
    }

    private void SetNextDialog()
    {
        if (_currentDialogIndex + 1 >= dialogs.Length)
        {
            _isUpdateDialogue = true;
            return;
        }

        StartCoroutine(DialogTransitionSequence());
    }

    private IEnumerator DialogTransitionSequence()
    {
        _isTransitioning = true;
        speaker.objectArrow.SetActive(false);
        speaker.textDialog.text = "";

        _currentDialogIndex++;
        string currentVideoName = dialogs[_currentDialogIndex].videoName;

        // 첫 대사가 아니고, '이전 영상과 현재 영상의 이름이 다를 때'만 암전 및 플레이어 전환을 수행
        bool isVideoChanged = _currentDialogIndex > 0 &&
                              !string.IsNullOrEmpty(currentVideoName) &&
                              (dialogs[_currentDialogIndex - 1].videoName != currentVideoName);

        if (isVideoChanged)
        {
            // 영상이 바뀔 때만 화면을 까맣게 암전 (Fade Out)
            yield return StartCoroutine(FadePanel(speaker.fadePanel, 0f, 1f, fadeDuration));

            // 백그라운드에서 미리 로딩(Prepare) 중이던 nextVideoPlayer가 완료될 때까지 암전 상태로 대기
            while (!_nextVideoPlayer.isPrepared && !string.IsNullOrEmpty(currentVideoName))
            {
                yield return null;
            }

            // 암전된 블랙아웃 순간에 플레이어 및 UI 스왑
            _currentVideoPlayer.Stop();
            _currentRawImage.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(currentVideoName))
            {
                _nextRawImage.gameObject.SetActive(true);
                _nextVideoPlayer.Play();
                yield return new WaitForEndOfFrame(); // 첫 프레임 안착 대기
            }

            // 핑퐁 포인터 교체
            var tempPlayer = _currentVideoPlayer;
            _currentVideoPlayer = _nextVideoPlayer;
            _nextVideoPlayer = tempPlayer;

            var tempImage = _currentRawImage;
            _currentRawImage = _nextRawImage;
            _nextRawImage = tempImage;

            // 새로운 비디오 위로 암전 해제 (Fade In)
            yield return StartCoroutine(FadePanel(speaker.fadePanel, 1f, 0f, fadeDuration));
        }
        else if (_currentDialogIndex == 0)
        {
            // 완전 첫 대사 시작 시의 초기화 로직 (시작할 때만 Fade In)
            if (!string.IsNullOrEmpty(currentVideoName))
            {
                _currentVideoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, currentVideoName);
                _currentVideoPlayer.Prepare();
                while (!_currentVideoPlayer.isPrepared) yield return null;

                _currentRawImage.gameObject.SetActive(true);
                _currentVideoPlayer.Play();
                yield return new WaitForEndOfFrame();
            }
            yield return StartCoroutine(FadePanel(speaker.fadePanel, 1f, 0f, fadeDuration));
        }
        // 비디오가 같거나 없는 경우, 위의 if/else if를 모두 건너뛰므로 페이드와 플레이어 전환 없이 프리패스합니다.

        // 화면이 완전히 밝아지면 텍스트 타이핑 시작
        _isTransitioning = false;
        _typingRoutine = StartCoroutine(CoTypeText());

        // 유저가 이 자막을 읽기 시작한 순간부터, 다음 대사(Index + 1)의 영상을 백그라운드에서 미리 로드 시킴
        PrepareNextVideoNextFrame();
    }

    // 유저가 자막을 읽는 동안 다음 영상(핑퐁 플레이어)을 백그라운드 GPU 메모리에 미리 올리는 핵심 함수
    private void PrepareNextVideoNextFrame()
    {
        int nextIndex = _currentDialogIndex + 1;
        if (nextIndex < dialogs.Length)
        {
            string currentVideoName = dialogs[_currentDialogIndex].videoName;
            string nextVideoName = dialogs[nextIndex].videoName;

            // 다음 영상이 존재하고, 현재 재생 중인 영상과 파일명이 다를 때만 핑퐁 준비 동작
            if (!string.IsNullOrEmpty(nextVideoName) && nextVideoName != currentVideoName)
            {
                _nextVideoPlayer.Stop();
                _nextVideoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, nextVideoName);
                _nextVideoPlayer.Prepare(); // 백그라운드 비동기 로드 시작
            }
        }
    }

    private IEnumerator FadePanel(Image image, float fromAlpha, float toAlpha, float duration)
    {
        image.gameObject.SetActive(true);
        float elapsed = 0f;
        Color color = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        image.color = new Color(color.r, color.g, color.b, toAlpha);

        if (toAlpha <= 0f) image.gameObject.SetActive(false);
    }

    private IEnumerator CoTypeText()
    {
        int index = 0;
        _isTypingEffect = true;
        string targetDialog = dialogs[_currentDialogIndex].dialog;

        while (index <= targetDialog.Length)
        {
            speaker.textDialog.text = targetDialog.Substring(0, index);
            ++index;
            yield return new WaitForSeconds(typeSpeed);
        }

        _isTypingEffect = false;
        speaker.objectArrow.SetActive(true);
    }

    private IEnumerator FadePanelAndGoToNext(Image image, float fromAlpha, float toAlpha, float duration)
    {
        _isEnd = true;
        image.gameObject.SetActive(true);

        float elapsed = 0f;
        Color color = image.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        image.color = new Color(color.r, color.g, color.b, toAlpha);

        videoPlayerA.Stop();
        videoPlayerB.Stop();
        GameManager.Instance.StartGame();
    }

    void PlayerActions.IUIActionMapActions.OnNextDialogue(InputAction.CallbackContext context)
    {
        if (!context.started || _isEnd) return;
        if (_isTransitioning) return;

        if (_isTypingEffect)
        {
            _isTypingEffect = false;
            if (_typingRoutine != null) StopCoroutine(_typingRoutine);
            speaker.textDialog.text = dialogs[_currentDialogIndex].dialog;
            speaker.objectArrow.SetActive(true);
            return;
        }

        SetNextDialog();
    }

    //private IEnumerator FadeImage(RawImage image, float fromAlpha, float toAlpha, float duration)
    //{
    //    float elapsed = 0f;
    //    Color color = image.color;
    //    while (elapsed < duration)
    //    {
    //        elapsed += Time.deltaTime;
    //        float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
    //        image.color = new Color(color.r, color.g, color.b, alpha);
    //        yield return null;
    //    }
    //    image.color = new Color(color.r, color.g, color.b, toAlpha);
    //}

    //private IEnumerator SetNextDialogWithFade(bool isFirst = false)
    //{
    //    speaker.textDialog.text = "";
    //    speaker.objectArrow.SetActive(false);
    //    _isFadeOut = true;

    //    // 페이드 아웃
    //    _fadeRoutine = StartCoroutine(FadeImage(speaker.imageScene, 1f, 0f, fadeDuration));
    //    yield return _fadeRoutine;
    //    _isFadeOut = false;

    //    if (isFirst)
    //    {
    //        speaker.imageScene.gameObject.SetActive(true);
    //    }

    //    ++_currentDialogIndex;
    //    videoPlayer.Stop();
    //    videoPlayer.source = VideoSource.Url;
    //    // videoPlayer.clip = dialogs[_currentDialogIndex].video;
    //    videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, dialogs[_currentDialogIndex].videoName);
    //    _isFadeIn = true;

    //    yield return new WaitForSeconds(0.2f);

    //    // 페이드 인
    //    _fadeRoutine = StartCoroutine(FadeImage(speaker.imageScene, 0f, 1f, fadeDuration));
    //    yield return _fadeRoutine;
    //    _isFadeIn = false;

    //    // 텍스트 타이핑 시작
    //    videoPlayer.Play();
    //    StartCoroutine("CoTypeText");
    //}
}
