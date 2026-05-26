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
    private bool _isEnd;
    private Coroutine _typingRoutine;

    private PlayerActions _uiActions;
    private bool _isUpdateDialogue;

    // 현재 상영 중인 플레이어와 다음에 상영될 플레이어를 가리키는 포인터 필드
    private VideoPlayer _currentVideoPlayer;
    private VideoPlayer _nextVideoPlayer;
    private RawImage _currentRawImage;
    private RawImage _nextRawImage;

    private bool _isFirstVideoReady = false; // 첫 영상 준비 완료 플래그

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        if (isAutoStart)
        {
            // 무조건 첫 영상이 렌더링을 시작할 때까지 대기하지만, 
            // 0.5초 대기 방어선을 두어 게임이 멈추거나 RawImage가 안 켜지는 현상을 원천 차단합니다.
            StartCoroutine(SafeLoadFirstVideoRoutine());
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

    private void OnEnable()
    {
        if (_uiActions != null)
        {
            _uiActions.UIActionMap.Enable();
        }
    }

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

        _currentVideoPlayer = videoPlayerA;
        _nextVideoPlayer = videoPlayerB;
        _currentRawImage = speaker.imageSceneA;
        _nextRawImage = speaker.imageSceneB;

        // 에디터 잔재 세탁
        videoPlayerA.Stop();
        videoPlayerB.Stop();
        ConfigureVideoPlayer(videoPlayerA);
        ConfigureVideoPlayer(videoPlayerB);

        // 투명도(Alpha) 조작을 완전히 제거하고, 기존처럼 확실하게 활성화 상태와 컬러를 강제 고정합니다.
        _currentRawImage.gameObject.SetActive(true);
        _nextRawImage.gameObject.SetActive(false); // 다음 대사 영상은 우선 꺼둠

        _currentRawImage.color = Color.white;
        _nextRawImage.color = Color.white;

        if (speaker.fadePanel != null)
        {
            speaker.fadePanel.gameObject.SetActive(true);
            speaker.fadePanel.color = new Color(0, 0, 0, 1f); // 페이드 패널만 까맣게 덮어서 로딩 감춤
        }

        if (dialogs != null && dialogs.Length > 0 && !string.IsNullOrEmpty(dialogs[0].videoName))
        {
            _currentVideoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, dialogs[0].videoName);
            _currentVideoPlayer.Prepare();
        }
    }

    private IEnumerator SafeLoadFirstVideoRoutine()
    {
        _isTransitioning = true; // 입력 잠금

        // 영상 재생 명령 선제 가동
        _currentVideoPlayer.Play();

        // 비디오 플레이어가 첫 프레임을 디코딩할 때까지 최대 0.5초만 대기
        // 에디터가 꼬여서 이 신호가 안 오더라도 0.5초 뒤에는 무조건 탈출하므로 프리징이 없습니다.
        float safetyTimeout = 0.5f;
        while (_currentVideoPlayer.frame < 0 && safetyTimeout > 0f)
        {
            safetyTimeout -= Time.deltaTime;
            yield return null;
        }

        // 혹시 모를 에디터 오동작 대비 RawImage 상태 재확인 강제 주입
        _currentRawImage.gameObject.SetActive(true);

        // 첫 영상이 돌기 시작했으므로 어두운 화면을 걷어냅니다.
        yield return StartCoroutine(FadePanel(speaker.fadePanel, 1f, 0f, fadeDuration));

        // 본격적인 다이얼로그 루프 시동 (0번 인덱스 처리를 위해 -1 셋팅)
        _currentDialogIndex = -1;
        StartCoroutine(DialogTransitionSequence());
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

        if (_currentDialogIndex >= dialogs.Length)
        {
            _isTransitioning = false;
            yield break;
        }

        string currentVideoName = dialogs[_currentDialogIndex].videoName;

        // 0번 인덱스는 SafeLoadFirstVideoRoutine에서 이미 RawImage를 켜고 영상을 틀었으므로 스킵
        if (_currentDialogIndex > 0)
        {
            bool isVideoChanged = !string.IsNullOrEmpty(currentVideoName) &&
                                  (dialogs[_currentDialogIndex - 1].videoName != currentVideoName);

            if (isVideoChanged)
            {
                // 암전 처리
                yield return StartCoroutine(FadePanel(speaker.fadePanel, 0f, 1f, fadeDuration));

                if (!string.IsNullOrEmpty(currentVideoName))
                {
                    // 기존 벨님의 안정적인 핑퐁 로직 유지
                    _nextRawImage.gameObject.SetActive(true);
                    _nextVideoPlayer.Play();

                    // 다음 영상이 첫 프레임을 안전하게 그릴 때까지 살짝 대기 (최대 0.5초 방어)
                    float nextVideoTimeout = 0.5f;
                    while (_nextVideoPlayer.frame < 0 && nextVideoTimeout > 0f)
                    {
                        nextVideoTimeout -= Time.deltaTime;
                        yield return null;
                    }
                }

                _currentVideoPlayer.Stop();
                _currentRawImage.gameObject.SetActive(false); // 사용이 끝난 이전 이미지는 비활성화

                // 포인터 체인지
                var tempPlayer = _currentVideoPlayer;
                _currentVideoPlayer = _nextVideoPlayer;
                _nextVideoPlayer = tempPlayer;

                var tempImage = _currentRawImage;
                _currentRawImage = _nextRawImage;
                _nextRawImage = tempImage;

                // 암전 해제
                yield return StartCoroutine(FadePanel(speaker.fadePanel, 1f, 0f, fadeDuration));
            }
        }

        _isTransitioning = false;
        _typingRoutine = StartCoroutine(CoTypeText());

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

        _uiActions.UIActionMap.Disable();
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

    void PlayerActions.IUIActionMapActions.OnSkip(InputAction.CallbackContext context)
    {

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
