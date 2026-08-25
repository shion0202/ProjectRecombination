using Managers;
using System.Collections;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    /// <summary>
    /// 게임이 실제 플레이 상태에 들어간 뒤 IsOn이 되는 Input.
    /// 시퀀스의 시작점으로 쓴다. (Trigger처럼 플레이어가 밟아줄 필요가 없다.)
    ///
    /// GameManager가 Playing 상태가 되는 시점은 PlayerController.PlayIntroSequence()가
    /// 시작되는 시점이기도 하므로, 카메라 연출과 HUD 활성화가 끝날 때까지
    /// delayAfterPlaying 만큼 더 기다린 뒤 신호를 낸다.
    /// (StartGame()이 넘기는 연출 길이는 4초이므로 기본값은 그보다 약간 크게 잡았다.)
    /// </summary>
    public class OnPlayStarted : ProcessBase
    {
        [Tooltip("Playing 상태 진입 후 추가로 대기할 시간(초). 인트로 카메라 연출이 끝나기를 기다린다.")]
        [SerializeField] private float delayAfterPlaying = 4.5f;

        private bool _isWaiting;

        // 이 노드는 그래프의 시작점이라 다른 노드가 Execute()를 불러주지 않는다.
        // 스스로 Playing 상태를 기다린다.
        private void Update()
        {
            if (IsOn || _isWaiting) return;
            if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

            _isWaiting = true;
            StartCoroutine(WaitAndSignal());
        }

        private IEnumerator WaitAndSignal()
        {
            yield return new WaitForSeconds(delayAfterPlaying);

            IsOn = true;
            Debug.Log("[OnPlayStarted] 시퀀스 시작 신호");
        }

        // 그래프에서 수동으로 시작시키고 싶을 때를 위한 경로.
        public override void Execute()
        {
            if (IsOn || _isWaiting) return;

            _isWaiting = true;
            StartCoroutine(WaitAndSignal());
        }
    }
}
