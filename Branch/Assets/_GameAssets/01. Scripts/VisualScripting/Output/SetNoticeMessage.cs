using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    public class SetNoticeMessage : ProcessBase
    {
        // GameUIController.ActivateMessage 의 기본 인자와 맞춘 값.
        // 기존 노드들이 모두 이 값으로 동작해 왔으므로 바꾸지 않는다.
        private const float DefaultActiveTime = 5.0f;

        [SerializeField] private string noticeMessage;
        [SerializeField] private string enNoticeMessage;

        [Tooltip("메시지를 보여주는 시간(초). 앞뒤로 페이드 인/아웃이 각각 1초씩 더 붙는다. " +
                 "기본값 5초는 기존 동작과 동일하다.")]
        [SerializeField] private float displayDuration = DefaultActiveTime;

        /// <summary>
        /// 실제 사용될 유지 시간. 기존 씬의 노드가 이 필드 없이 역직렬화되어 0이 들어오더라도
        /// 기본값으로 떨어지게 해, 본편 메시지 동작이 바뀌지 않도록 한다.
        /// </summary>
        private float ActiveTime => displayDuration > 0.0f ? displayDuration : DefaultActiveTime;

        public override void Execute()
        {
            if (IsOn) return;

            Managers.GUIManager.Instance.GameUIController.ActivateMessage(
                LocalizationManager.IsKorean ? noticeMessage : enNoticeMessage, ActiveTime);

            IsOn = true;
        }
    }
}