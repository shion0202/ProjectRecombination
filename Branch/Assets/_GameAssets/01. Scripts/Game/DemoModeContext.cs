using UnityEngine;

/// <summary>
/// 체험 플레이(Demo) 전용 전역 설정 홀더. 본편에서는 기본값(1.0)을 유지하므로 영향이 없다.
/// TestManager와 동일한 정적 클래스 패턴이며, 스탯 초기화 핫패스에서 싱글톤 조회 없이 바로 읽는다.
///
/// 값은 Blackboard.InitMonsterStatsByID()가 시트에서 스탯을 채운 "직후"에 적용된다.
/// Init()이 여러 번 호출되어도 매번 시트에서 원본을 새로 읽으므로 배수가 중첩되지 않는다.
///
/// 주입 시점은 GameManager.EnterPrologue()다. 스테이지 씬이 로드되기 전이므로
/// 씬 내 컴포넌트 Awake 순서와 무관하게 항상 몬스터 초기화보다 앞선다.
/// </summary>
public static class DemoModeContext
{
    private const string ProfileResourcePath = "Demo/DemoBossProfile";

    public static bool IsActive;
    public static float BossHealthMultiplier = 1f;
    public static float BossDamageMultiplier = 1f;

    /// <summary>
    /// Resources에서 프로필을 읽어 적용한다. 에셋이 없으면 배수 1.0으로 활성화만 한다.
    /// </summary>
    public static void LoadAndApply()
    {
        Apply(Resources.Load<DemoBossProfile>(ProfileResourcePath));
    }

    public static void Apply(DemoBossProfile profile)
    {
        IsActive = true;

        if (profile == null)
        {
            BossHealthMultiplier = 1f;
            BossDamageMultiplier = 1f;
            Debug.LogWarning($"[DemoModeContext] '{ProfileResourcePath}' 에셋을 찾지 못해 기본 배수(1.0)를 사용합니다.");
            return;
        }

        BossHealthMultiplier = profile.healthMultiplier;
        BossDamageMultiplier = profile.damageMultiplier;

        Debug.Log($"[DemoModeContext] 배수 적용 - HP x{BossHealthMultiplier}, DMG x{BossDamageMultiplier}");
    }

    public static void Reset()
    {
        IsActive = false;
        BossHealthMultiplier = 1f;
        BossDamageMultiplier = 1f;
    }
}
