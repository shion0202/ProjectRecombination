using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    // 상호작용 키를 통해 호출할 이벤트 등록을 해제하는 Output
    public class Unregister : ProcessBase
    {
        [SerializeField] private ProcessBase nextInput;

        public override void Execute()
        {
            IsOn = true;

            // To-do: PlayerController와 의존성을 가지므로 추후 Event Manager 등으로 분리
            PlayerController.UnregisterEvent(nextInput.Execute);
        }
    }
}
