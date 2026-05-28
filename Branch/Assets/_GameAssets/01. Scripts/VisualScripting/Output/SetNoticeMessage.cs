using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class SetNoticeMessage : ProcessBase
    {
        [SerializeField] private string noticeMessage;
        [SerializeField] private string enNoticeMessage;

        public override void Execute()
        {
            if (IsOn) return;

            Managers.GUIManager.Instance.GameUIController.ActivateMessage(LocalizationManager.IsKorean ? noticeMessage : enNoticeMessage);

            IsOn = true;
        }
    }
}