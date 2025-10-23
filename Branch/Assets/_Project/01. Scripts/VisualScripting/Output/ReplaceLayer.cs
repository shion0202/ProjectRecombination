using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    // 레이어 교체 GOS
    // 주로 Outline 추가/제거 용도로 사용할 예정
    public class ReplaceLayer : ProcessBase
    {
        [SerializeField] private GameObject target;
        [SerializeField] private LayerMask newLayer;

        public override void Execute()
        {
            if (IsOn) return;

            target.layer = newLayer;

            IsOn = true;
        }
    }
}
