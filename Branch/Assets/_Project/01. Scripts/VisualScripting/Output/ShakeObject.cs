using UnityEngine;
using DG.Tweening;

namespace _Project.Scripts.VisualScripting
{
    public class ShakeObject : ProcessBase
    {
        [SerializeField] private GameObject objectToShake;
        [SerializeField] private float duration = 1f; // 흔드는 시간
        [Range(0.01f, 0.1f)][SerializeField] private float magnitude = 0.1f; // 흔드는 강도
        [SerializeField] private bool repeat;
        
        public override void Execute()
        {
            // 이미 흔들기가 진행 중이면 무시
            if (IsOn) return;
            if (objectToShake is null) return;
            // RunningCoroutine = StartCoroutine(C_Shake());
            
            Debug.Log($"{objectToShake.transform.position.ToString()}");
            
            IsOn = true;
            if (!repeat)
                objectToShake.transform.DOShakePosition(duration, magnitude).OnComplete(() =>
                {
                    IsOn = false;
                });
            else
                objectToShake.transform.DOShakePosition(duration, magnitude).SetLoops(-1);
        }
    }
}