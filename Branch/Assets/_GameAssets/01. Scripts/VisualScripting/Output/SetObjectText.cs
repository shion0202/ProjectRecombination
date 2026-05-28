using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class SetObjectText : ProcessBase
    {
        [SerializeField] private Transform target;
        [SerializeField, TextArea(3, 5)] private string objectDescription;
        [SerializeField, TextArea(3, 5)] private string enObjectDescription;

        public override void Execute()
        {
            if (IsOn) return;

            Managers.GUIManager.Instance.GameUIController.ObjectText.text = LocalizationManager.IsKorean ? objectDescription : enObjectDescription;
            Managers.LocalizationManager.CurrentObjective[0] = objectDescription;
            Managers.LocalizationManager.CurrentObjective[1] = enObjectDescription;

            if (target)
            {
                Managers.GUIManager.Instance.GameUIController.SetIndicatorTarget(target);
                Managers.GUIManager.Instance.GameUIController.SetIndicator(true);
            }

            IsOn = true;
        }
    }
}