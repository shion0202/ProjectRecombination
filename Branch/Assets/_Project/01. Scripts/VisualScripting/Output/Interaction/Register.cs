using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    // 상호작용 키를 통해 호출할 이벤트를 등록하는 Output
    // OnEvent Input을 nextInput 변수에 연결 (Input → Output(Register) → Input(OnEvent) → Output 구조)
    public class Register : ProcessBase
    {
        [SerializeField] private ProcessBase nextInput;

        public override void Execute()
        {
            IsOn = true;

            // PlayerController와 의존성을 가지므로 추후 Event Manager 등으로 분리
            PlayerController.RegisterEvent(nextInput.Execute);
        }
    }
}
