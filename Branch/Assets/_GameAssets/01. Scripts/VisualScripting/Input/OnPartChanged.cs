using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public enum EPartChangeType
    {
        Acquired,   // 파츠 상자에서 F로 획득
        Equipped,   // 파츠 교체 창(Tab)에서 실제로 교체
    }

    /// <summary>
    /// 플레이어가 파츠를 획득하거나 교체하면 IsOn이 되는 Input.
    ///
    /// 입력 액션만으로는 "F를 눌렀다" / "Tab을 눌렀다"까지만 알 수 있고
    /// 실제로 획득/교체가 일어났는지는 알 수 없어서, Inventory의 이벤트를 구독한다.
    /// Inventory.EquipItem은 이미 장착 중인 파츠를 다시 고르면 앞쪽 가드에서 빠져나가므로,
    /// Equipped는 실제로 장착이 바뀐 경우에만 발생한다.
    ///
    /// Timer / OnPlayerAction과 같이 Execute()를 받아야 감지를 시작한다.
    /// </summary>
    public class OnPartChanged : ProcessBase
    {
        [SerializeField] private EPartChangeType changeType;

        [Tooltip("특정 부위만 인정하려면 지정한다. 비워두면(None) 어떤 부위든 인정한다.")]
        [SerializeField] private bool filterByPartType;
        [SerializeField] private EPartType requiredPartType;

        [Tooltip("체크하면 오브젝트가 활성화되는 즉시 감지를 시작한다.")]
        [SerializeField] private bool autoStart;

        private bool _isDetecting;

        private void OnEnable()
        {
            _isDetecting = false;

            if (autoStart) BeginDetect();
        }

        private void OnDisable() => EndDetect();

        private void OnDestroy() => EndDetect();

        public override void Execute()
        {
            if (IsOn) return;
            BeginDetect();
        }

        private void BeginDetect()
        {
            if (_isDetecting) return;
            _isDetecting = true;

            if (changeType == EPartChangeType.Acquired) Inventory.OnItemAcquired += OnPart;
            else Inventory.OnItemEquipped += OnPart;
        }

        private void EndDetect()
        {
            if (!_isDetecting) return;
            _isDetecting = false;

            if (changeType == EPartChangeType.Acquired) Inventory.OnItemAcquired -= OnPart;
            else Inventory.OnItemEquipped -= OnPart;
        }

        private void OnPart(PartBase part)
        {
            if (IsOn || part == null) return;
            if (filterByPartType && part.PartType != requiredPartType) return;

            IsOn = true;
            EndDetect();

            Debug.Log($"[OnPartChanged] {changeType} 조건 충족 ({part.PartType})");
        }
    }
}
