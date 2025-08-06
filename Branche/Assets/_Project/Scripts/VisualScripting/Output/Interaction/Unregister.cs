using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class Unregister : ProcessBase
    {
        [SerializeField] private ProcessBase nextInput;

        public override void Execute()
        {
            IsOn = true;

            PlayerController.UnregisterEvent(nextInput.Execute);
        }
    }
}
