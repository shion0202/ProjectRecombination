using Managers;
using Monster.AI.FSM;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    /// <summary>
    /// 아몬 2페이즈를 1페이즈 없이 단독 기동하는 Output.
    /// 체험 플레이는 1페이즈를 건너뛰므로 DungeonManager.AmonSecondPhase() 경로를 쓸 수 없다.
    ///
    /// AmonPaseTwoFSM은 사망 시 DungeonManager.AmonEndPhase()를 호출하므로,
    /// FSM을 켜기 전에 DungeonManager에 참조를 주입해둔다.
    /// (본편에서는 BossTrigger가 하는 일이다.)
    /// </summary>
    public class StartAmonSecondPhase : ProcessBase
    {
        [Tooltip("씬에 배치된 아몬 2페이즈의 FSM. isEnabled는 꺼둔 상태로 둔다.")]
        [SerializeField] private FSM amonSecondPhase;

        [Tooltip("아몬 처치 후 플레이어가 돌아갈 지점. 데모는 곧바로 크레딧으로 가므로 비워둬도 된다.")]
        [SerializeField] private Transform playerRespawnPoint;

        public override void Execute()
        {
            if (IsOn) return;

            if (amonSecondPhase == null)
            {
                Debug.LogWarning("[StartAmonSecondPhase] amonSecondPhase가 할당되지 않아 보스를 기동할 수 없습니다.");
                return;
            }

            DungeonManager.Instance.AmonSecondPhasePrefab = amonSecondPhase;
            if (playerRespawnPoint != null)
                DungeonManager.Instance.PlayerRespawnPoint = playerRespawnPoint;

            amonSecondPhase.isEnabled = true;

            IsOn = true;

            Debug.Log("[StartAmonSecondPhase] 아몬 2페이즈 기동");
        }
    }
}
