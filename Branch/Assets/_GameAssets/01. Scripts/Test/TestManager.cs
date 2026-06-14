/// <summary>
/// 테스트 편의를 위한 전역 설정 홀더. 빌드/일반 플레이에서는 기본값을 유지하므로 영향이 없고,
/// SceneTestLauncher 등 에디터 테스트 툴에서만 값을 변경한다.
/// (정적 클래스라 씬 오브젝트/싱글톤 없이 데미지 핫패스에서 바로 읽을 수 있다.)
/// </summary>
public static class TestManager
{
    /// <summary>
    /// '적/오브젝트가 받는 데미지' 배수. 기본 1 (영향 없음).
    /// 테스트 중 플레이어가 몬스터에게 주는 데미지를 키워 빠르게 처치할 때 사용한다.
    /// FSM.OnHit / DamagableObject.ApplyDamage 의 데미지 계산에 곱해진다.
    /// </summary>
    public static float EnemyDamageMultiplier = 1f;

    /// <summary>
    /// 테스트 무적 모드(SceneTestInvincibility)가 부여한 플레이어 무적이 활성 상태인지 여부. 기본 false.
    /// 게임 로직(예: AmonLockdown, SafeZoneObject)이 플레이어 무적을 해제할 때 이 값이 true 이면
    /// 해제를 건너뛰어, 테스트 매니저가 부여한 무적이 유지되도록 한다.
    /// </summary>
    public static bool PlayerInvincible = false;
}
