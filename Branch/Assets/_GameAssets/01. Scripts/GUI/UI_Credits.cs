using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 엔딩 크레딧 스크롤. 끝까지 올라가면 타이틀로 복귀한다.
///
/// 본편과 체험 플레이가 이 크레딧을 공유한다.
///  - 본편: 게임씬 → 에필로그 → 크레딧 → 타이틀
///  - 데모: 데모씬 → 크레딧 → 타이틀 (에필로그 생략)
/// 복귀 경로가 다르므로 GameManager.PlayMode로 분기한다.
///
/// 재생 길이는 "속도 + 도착 좌표"가 아니라 scrollDuration(초)으로 직접 지정한다.
/// 속도는 이동 거리 ÷ 시간으로 매번 계산되므로, 크레딧 본문이 길어지거나 짧아져도
/// 재생 시간은 지정한 값 그대로 유지된다.
/// </summary>
public class UI_Credits : MonoBehaviour, PlayerActions.IUIActionMapActions
{
    [Header("재생 시간")]
    [Tooltip("크레딧이 처음부터 끝까지 흐르는 데 걸리는 시간(초). 재생 길이는 이 값만 조절하면 된다.")]
    [SerializeField] private float scrollDuration = 30.0f;

    [Tooltip("스크롤이 끝난 뒤 타이틀로 넘어가기 전 잠시 멈춰 있는 시간(초).")]
    [SerializeField] private float holdAtEndSeconds = 1.0f;

    [Header("이동 거리")]
    [Tooltip("스크롤되는 크레딧 본문. 지정하면 (본문 높이 + 화면 높이)로 이동 거리를 자동 계산해, " +
             "본문이 길어져도 항상 끝까지 흐른 뒤 종료된다. 비워두면 아래 수동 값을 쓴다.")]
    [SerializeField] private RectTransform content;

    [Tooltip("content를 비워둘 때 사용할 이동 거리(px).")]
    [SerializeField] private float manualTravelDistance = 1600.0f;

    // 스크롤 시작 위치. 재생이 끝나면 여기로 되돌려 다음 재생이 정상 동작하게 한다.
    private Vector3 _startPosition;
    private bool _isStartPositionCaptured;

    private float _travelDistance;
    private float _elapsed;
    private float _holdRemaining;

    // 종료 처리를 한 번만 실행하기 위한 플래그.
    // 없으면 목표 지점 도달 이후 매 프레임 복귀 함수가 호출된다(둘 다 async void라 중첩 실행된다).
    private bool _isFinished;

    private PlayerActions _uiActions;

    private void Awake()
    {
        CaptureStartPosition();

        _uiActions = new PlayerActions();
        _uiActions.UIActionMap.SetCallbacks(this);
    }

    private void OnEnable()
    {
        // 이 오브젝트는 InitUI가 한 번만 생성하고 이후에는 SetActive로 토글되므로,
        // 재생 상태를 활성화 시점마다 초기화해야 두 번째 재생이 정상 동작한다.
        CaptureStartPosition();

        transform.position = _startPosition;
        _travelDistance = CalculateTravelDistance();
        _elapsed = 0.0f;
        _holdRemaining = holdAtEndSeconds;
        _isFinished = false;

        _uiActions?.UIActionMap.Enable();
    }

    private void OnDisable() => _uiActions?.UIActionMap.Disable();

    private void OnDestroy()
    {
        _uiActions?.UIActionMap.Disable();
        _uiActions?.Dispose();
    }

    // 클릭(NextDialogue) 또는 ESC(Skip)로 크레딧을 건너뛴다.
    // 행사에서는 앞 체험자가 크레딧을 끝까지 보지 않고 자리를 떠도
    // 다음 체험자가 재생이 끝날 때까지 기다리게 되므로, 즉시 넘길 수단이 필요하다.
    void PlayerActions.IUIActionMapActions.OnNextDialogue(InputAction.CallbackContext context) => TrySkip(context);

    void PlayerActions.IUIActionMapActions.OnSkip(InputAction.CallbackContext context) => TrySkip(context);

    private void TrySkip(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_isFinished) return;

        Debug.Log("[UI_Credits] 크레딧 스킵");
        Finish();
    }

    private void CaptureStartPosition()
    {
        if (_isStartPositionCaptured) return;

        _startPosition = transform.position;
        _isStartPositionCaptured = true;
    }

    /// <summary>
    /// 본문이 화면 아래에서 시작해 위로 완전히 빠져나갈 때까지의 거리.
    /// content를 지정하지 않으면 수동 값을 사용한다.
    /// </summary>
    private float CalculateTravelDistance()
    {
        if (content == null) return manualTravelDistance;

        // Screen Space - Overlay 캔버스에서는 월드 좌표 1 = 화면 픽셀 1이므로
        // 본문 높이(lossyScale 반영)와 Screen.height를 같은 단위로 더할 수 있다.
        float contentHeight = content.rect.height * content.lossyScale.y;
        return contentHeight + Screen.height;
    }

    private void Update()
    {
        if (_isFinished) return;

        if (_elapsed < scrollDuration)
        {
            _elapsed += Time.deltaTime;

            float progress = scrollDuration <= 0.0f ? 1.0f : Mathf.Clamp01(_elapsed / scrollDuration);
            transform.position = _startPosition + new Vector3(0.0f, _travelDistance * progress, 0.0f);
            return;
        }

        // 스크롤 종료 후 잠시 대기
        if (_holdRemaining > 0.0f)
        {
            _holdRemaining -= Time.deltaTime;
            return;
        }

        Finish();
    }

    private void Finish()
    {
        _isFinished = true;
        _uiActions?.UIActionMap.Disable();

        if (GameManager.Instance.PlayMode == EPlayMode.Demo)
        {
            // 체험 플레이는 스테이지/플레이어 씬과 세션 상태를 되돌린 뒤 타이틀로 간다.
            GameManager.Instance.ReturnToTitleFromDemo();
        }
        else
        {
            GameManager.Instance.EnterTitle();
        }
    }
}
