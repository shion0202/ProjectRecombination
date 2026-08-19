/// <summary>
/// SceneTestLauncher 의 인게임 셋업(Pool/Player/시작위치)이 끝난 직후 호출되는 테스트 전용 훅.
/// 씬에 배치된 구현 컴포넌트가 '테스트 시작 시 동작'(보스 FSM 활성화, 웨이브 스폰 등)을 정의한다.
/// 실제 게임에서 선행 조건(예: 아몬 1페이즈 사망)으로만 켜지는 로직을,
/// 테스트 씬에서는 선행 조건 없이 바로 확인할 수 있게 하는 용도다.
/// </summary>
public interface ISceneTestHook
{
    void OnTestStart();
}
