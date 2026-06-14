using UnityEngine;

/// <summary>
/// 테스트 중 플레이어를 계속 무적 상태로 유지한다. SceneTestLauncher 의 '무적 모드' 토글이 켜져 있으면
/// 인게임 셋업 직후 플레이어 오브젝트에 동적으로 부착된다.
///
/// 단순히 한 번 SetPlayerState(Invincibility, true) 하면 대시 i-frame 종료 등으로 플래그가 해제될 수 있어,
/// 매 프레임(LateUpdate) 다시 설정해 테스트 내내 무적을 보장한다. (PlayerController.TakeDamage 참고)
/// </summary>
[DisallowMultipleComponent]
public class SceneTestInvincibility : MonoBehaviour
{
    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void LateUpdate()
    {
        if (_player == null) return;

        _player.SetPlayerState(EPlayerState.Invincibility, true);
    }
}
