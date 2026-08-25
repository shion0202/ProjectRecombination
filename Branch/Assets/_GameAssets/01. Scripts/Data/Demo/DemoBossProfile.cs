using UnityEngine;

/// <summary>
/// 체험 플레이(Demo)용 몬스터 스탯 배수.
/// 구글 스프레드시트를 수정하지 않고 런타임에만 적용한다. 밸런싱은 이 에셋의 값만 바꿔 조절한다.
///
/// 에셋 위치는 Assets/Resources/Demo/DemoBossProfile.asset 으로 고정한다.
/// (DemoModeContext가 Resources.Load로 읽으므로 인스펙터 배선이 필요 없다.)
///
/// 기존 Resources/Tutorial 폴더는 도움말 시스템(TutorialDataSO, UI_Tutorial)이 쓰는 곳이라
/// 성격이 다르므로 섞지 않는다.
/// </summary>
[CreateAssetMenu(fileName = "DemoBossProfile", menuName = "Scriptable Object/Demo Boss Profile", order = 22)]
public class DemoBossProfile : ScriptableObject
{
    [Tooltip("최대 체력 배수. 1보다 작으면 약해진다.")]
    [Range(0.01f, 1f)] public float healthMultiplier = 0.4f;

    [Tooltip("공격력 배수. 1보다 작으면 약해진다.")]
    [Range(0.01f, 1f)] public float damageMultiplier = 0.6f;
}
