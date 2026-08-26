using Managers;
using Monster.AI.FSM;
using System.Collections;
using UnityEngine;

namespace _Project.Scripts.VisualScripting
{
    /// <summary>
    /// 아몬 2페이즈를 1페이즈 없이 단독 기동하는 Output.
    /// 체험 플레이는 1페이즈를 건너뛰므로 DungeonManager.AmonSecondPhase() 경로를 쓸 수 없다.
    ///
    /// AmonPaseTwoFSM은 사망 시 DungeonManager.AmonEndPhase()를 호출하므로,
    /// FSM을 켜기 전에 DungeonManager에 참조를 주입한다.
    /// (본편에서 BossTrigger가 하던 일이다.)
    ///
    /// 보스 오브젝트를 꺼둔 채로 배치해두면 이 노드가 켜준다.
    /// 활성화와 패턴 시작 사이에 한 프레임을 두는데, 활성화 프레임에
    /// FSM.OnEnable / Start → Blackboard.Init() 이 돌면서 타겟과 스탯, 상태가 잡히기 때문이다.
    /// 같은 프레임에 isEnabled 를 켜면 초기화가 끝나기 전에 Think/Act 가 돌 수 있다.
    /// </summary>
    public class StartAmonSecondPhase : ProcessBase
    {
        [Tooltip("씬에 배치된 아몬 2페이즈의 FSM. isEnabled 는 꺼둔 상태로 둔다.")]
        [SerializeField] private FSM amonSecondPhase;

        [Tooltip("등장 전까지 숨겨둘 보스 루트 오브젝트. 비워두면 FSM이 붙은 오브젝트를 사용한다. " +
                 "이미 활성 상태라면 그대로 둔다.")]
        [SerializeField] private GameObject bossRoot;

        [Tooltip("오브젝트를 켠 뒤 패턴을 시작하기까지의 추가 대기 시간(초). " +
                 "0이어도 한 프레임은 반드시 벌어진다. 등장 애니메이션 자체는 FSM의 spawnWaitTime이 담당한다.")]
        [SerializeField] private float enableDelay = 0.0f;

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

            IsOn = true;
            RunningCoroutine = StartCoroutine(ActivateRoutine());
        }

        private IEnumerator ActivateRoutine()
        {
            GameObject root = bossRoot != null ? bossRoot : amonSecondPhase.gameObject;

            if (!root.activeSelf)
            {
                root.SetActive(true);
                Debug.Log("[StartAmonSecondPhase] 보스 오브젝트 활성화");
            }

            // 활성화 프레임의 Awake / OnEnable / Start 가 모두 끝나도록 한 프레임 넘긴다.
            yield return null;

            if (enableDelay > 0.0f)
                yield return new WaitForSeconds(enableDelay);

            amonSecondPhase.isEnabled = true;

            Debug.Log("[StartAmonSecondPhase] 아몬 2페이즈 기동");
        }
    }
}
