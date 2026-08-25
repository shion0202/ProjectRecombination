/// <summary>
/// 플레이 모드. 행사 출품용 체험 플로우(Demo)와 본편(Normal)을 구분한다.
/// GameManager.EnterPrologue(EPlayMode) 시점에 결정되며, 타이틀 복귀 시 Normal로 되돌아간다.
/// </summary>
public enum EPlayMode
{
    Normal,
    Demo
}
