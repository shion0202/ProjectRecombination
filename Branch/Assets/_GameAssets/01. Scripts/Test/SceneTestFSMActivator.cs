using System.Collections.Generic;
using Monster.AI.FSM;
using UnityEngine;

/// <summary>
/// 씬 테스트(SceneTestLauncher) 인게임 셋업 완료 후 호출되어, 지정한 FSM(보스 등)을 직접 활성화한다.
/// 실제 게임에서는 페이즈 전환(예: 아몬 1페이즈 사망 → DungeonManager.AmonSecondPhase)으로만 켜지는
/// FSM 을, 테스트 씬에서는 선행 조건 없이 바로 동작 확인할 수 있도록 하는 테스트 전용 컴포넌트다.
///
/// [사용법] 테스트할 씬(예: 아몬 2페이즈 씬)에 빈 오브젝트를 만들어 이 컴포넌트를 붙이고,
/// fsmToEnable 에 켜고 싶은 FSM(예: AmonPaseTwoFSM)을 할당한다.
/// SceneTestLauncher 의 BootstrapInGame 모드로 실행하면 셋업 직후 자동으로 활성화된다.
/// </summary>
public class SceneTestFSMActivator : MonoBehaviour, ISceneTestHook
{
    [Tooltip("테스트 시작 시 isEnabled = true 로 켤 FSM 목록 (예: 아몬 2페이즈).")]
    [SerializeField] private List<FSM> fsmToEnable = new();

    public void OnTestStart()
    {
        foreach (FSM fsm in fsmToEnable)
        {
            if (fsm == null) continue;

            fsm.isEnabled = true;
            Debug.Log($"[SceneTest] FSM 활성화: {fsm.name}");
        }
    }
}
