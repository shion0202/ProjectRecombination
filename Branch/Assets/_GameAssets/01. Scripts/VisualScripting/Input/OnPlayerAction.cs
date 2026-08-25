using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.VisualScripting
{
    /// <summary>
    /// 감지할 플레이어 조작 종류.
    /// PlayerActions.inputactions 의 PlayerActionMap 액션과 1:1로 대응한다.
    /// </summary>
    public enum EPlayerActionType
    {
        Move,           // WASD 이동
        Look,           // 마우스 시점
        LeftAttack,     // 좌클릭 공격
        RightAttack,    // 우클릭 공격
        Dash,           // Shift (다리 파츠 스킬)
        ShoulderSkill,  // Space (등 파츠 스킬)
        RadialMenu,     // Tab (파츠 교체 창)
        Interaction,    // F (상호작용)
    }

    /// <summary>
    /// 플레이어가 지정한 조작을 충분히 수행하면 IsOn이 되는 Input.
    ///
    /// Timer와 같은 모양으로 동작한다. Execute()를 받아야 감지를 "시작"하므로,
    /// 그래프에서 해당 스텝에 도달했을 때부터만 판정된다.
    /// (그래야 인트로 도중에 미리 움직여서 스텝을 건너뛰는 일이 없다.)
    /// autoStart를 켜면 오브젝트가 활성화되는 즉시 감지한다.
    ///
    /// PlayerController를 건드리지 않고 PlayerActions를 별도 인스턴스로 구독한다.
    /// (UI_Prologue가 UIActionMap에 쓰는 것과 같은 방식이다.)
    /// </summary>
    public class OnPlayerAction : ProcessBase
    {
        [SerializeField] private EPlayerActionType actionType;

        [Tooltip("Move / Look 은 누적 입력량, 그 외 버튼류는 입력 횟수. " +
                 "Move는 1.0이 약 1초간 최대 입력, Look은 마우스 이동 픽셀의 누적이다.")]
        [SerializeField] private float requiredAmount = 1.0f;

        [Tooltip("체크하면 오브젝트가 활성화되는 즉시 감지를 시작한다. " +
                 "끄면 그래프에서 Execute()를 받은 뒤부터 감지한다.")]
        [SerializeField] private bool autoStart;

        private PlayerActions _actions;
        private InputAction _boundAction;
        private bool _isDetecting;
        private float _accumulated;

        private bool IsAnalog => actionType is EPlayerActionType.Move or EPlayerActionType.Look;

        private void Awake()
        {
            _actions = new PlayerActions();
            _boundAction = ResolveAction();
        }

        private void OnEnable()
        {
            // ProcessBase.OnEnable 이 IsOn 을 false 로 되돌린다. 여기서는 감지 상태만 초기화한다.
            _isDetecting = false;
            _accumulated = 0.0f;

            if (autoStart) BeginDetect();
        }

        private void OnDisable() => EndDetect();

        private void OnDestroy()
        {
            EndDetect();
            _actions?.Dispose();
        }

        public override void Execute()
        {
            if (IsOn) return;
            BeginDetect();
        }

        private void BeginDetect()
        {
            if (_isDetecting || _boundAction == null) return;

            _accumulated = 0.0f;
            _isDetecting = true;

            _boundAction.Enable();
            if (!IsAnalog) _boundAction.started += OnButtonPressed;
        }

        private void EndDetect()
        {
            if (!_isDetecting || _boundAction == null) return;

            _isDetecting = false;

            if (!IsAnalog) _boundAction.started -= OnButtonPressed;
            _boundAction.Disable();
        }

        private void Update()
        {
            if (!_isDetecting || IsOn) return;
            if (!IsAnalog) return;

            Vector2 value = _boundAction.ReadValue<Vector2>();

            // Move는 스틱/키 입력 크기라 시간을 곱해 "얼마나 오래 움직였는가"로 누적하고,
            // Look은 이미 프레임당 마우스 이동량이므로 그대로 누적한다.
            _accumulated += actionType == EPlayerActionType.Move
                ? value.magnitude * Time.deltaTime
                : value.magnitude;

            if (_accumulated >= requiredAmount) Satisfy();
        }

        private void OnButtonPressed(InputAction.CallbackContext context)
        {
            if (IsOn) return;

            _accumulated += 1.0f;
            if (_accumulated >= requiredAmount) Satisfy();
        }

        private void Satisfy()
        {
            IsOn = true;
            EndDetect();

            Debug.Log($"[OnPlayerAction] {actionType} 조건 충족");
        }

        private InputAction ResolveAction()
        {
            PlayerActions.PlayerActionMapActions map = _actions.PlayerActionMap;

            return actionType switch
            {
                EPlayerActionType.Move => map.Move,
                EPlayerActionType.Look => map.Look,
                EPlayerActionType.LeftAttack => map.LeftAttack,
                EPlayerActionType.RightAttack => map.RightAttack,
                EPlayerActionType.Dash => map.Dash,
                EPlayerActionType.ShoulderSkill => map.ShoulderSkill,
                EPlayerActionType.RadialMenu => map.RadialMenu,
                EPlayerActionType.Interaction => map.Interaction,
                _ => null,
            };
        }
    }
}
