using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class Register : ProcessBase
    {
        [SerializeField] private ProcessBase nextInput;

        public override void Execute()
        {
            IsOn = true;

            PlayerController.RegisterEvent(nextInput.Execute);
        }
    }
}
