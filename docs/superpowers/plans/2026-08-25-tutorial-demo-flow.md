# 튜토리얼 체험 플로우 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 타이틀의 별도 버튼으로 진입하는 5분 분량 체험 플로우(프롤로그 → 튜토리얼 → 저스펙 아몬 2페이즈 → 결과 → 타이틀)를 추가하고, 게임을 끄지 않고 반복 실행해도 안정적으로 동작하게 만든다.

**Architecture:** 매니저는 "어느 씬 세트를 쓰는가"만 알고, 5분 시퀀스의 내용은 전부 튜토리얼 씬 안의 `TutorialDirector`에 격리한다. 본편 변경은 모드 플래그 / 스테이지 세트 분기 / 스탯 배수 3줄 / 세션 리셋으로 한정한다. 보스 저스펙은 구글 시트를 수정하지 않고 런타임 배수로 처리한다.

**Tech Stack:** Unity 2022 LTS + URP, Addressables(씬 로드), Unity Input System, Cinemachine, DOTween

**Spec:** [2026-08-25-tutorial-demo-flow-design.md](../specs/2026-08-25-tutorial-demo-flow-design.md)

---

## Global Constraints

이 섹션의 제약은 **모든 태스크에 암묵적으로 적용된다.**

- **Active Scene은 `Scene_Persistent`로 고정되어 있으며 변경 금지.** 멀티 씬 환경에서 Active Scene이 바뀌면 베이크된 라이팅 결과와 컬링이 제대로 적용되지 않는다. `SceneController.SetActiveScene()` 호출부(`DungeonManager.cs:85`, `:148`, `:318`)의 주석 처리는 **의도된 설계이므로 해제하지 않는다.**
- **`SceneManager.MoveGameObjectToScene()` 사용 금지.** 위 제약과 같은 이유로 씬 소속을 옮기지 않는다. 세션 오브젝트 정리는 명시적 레지스트리(Task 2)로만 처리한다.
- **구글 스프레드시트(`CharacterStats` 등)를 수정하지 않는다.** 보스 스펙 조정은 런타임 배수로만 한다.
- **테스트 프레임워크를 새로 도입하지 않는다.** 이 프로젝트에는 게임 코드용 테스트 어셈블리가 없다(`.asmdef`는 `IDamagable` 하나, `manifest.json`에 `testables` 없음). 각 태스크의 검증은 **에디터 수동 확인 + Console 로그 단언**으로 수행한다. 각 태스크는 로그 문자열과 기대 출력을 명시한다.
- **씬 로드 순서:** `EnterPrologue()`는 **플레이어 씬을 스테이지 씬보다 먼저** 로드해야 한다. 씬에 배치된 몬스터는 `Blackboard.Init()`에서 `Target = MonsterManager.Instance.Player`를 한 번 읽고 굳으며, 재초기화 계기가 없다. Player가 없으면 `Target`이 null로 남아 `FSM.Think()` / `Act()`가 첫 줄에서 return해 **그 몬스터가 통째로 정지한다.** (2026-08-26 실측 확인: 데모룸의 아몬이 체력 0에도 반응하지 않던 원인. 본편은 아몬이 8스테이지에 있어 드러나지 않았고, `SceneTestLauncher.LoadInGame()`은 처음부터 올바른 순서를 쓰고 있었다.)
- **씬 주소 규약:** 스테이지 성격 씬은 Addressables `GameScene` 그룹, 시스템 씬(`Scene_Persistent` / `Scene_Player` / `Scene_UI`)은 `Default Local Group`에 등록되어 있다.
- **튜토리얼 씬은 기존 `0th_Demo_Room.unity`를 사용한다** (실행 중 결정). 경로 `Branch/Assets/_GameAssets/04. Scenes/GameScene/0th_Demo_Room.unity`, 주소 **`0th_Demo_Room`**, 그룹 `GameScene`. 계획서 본문의 `Tutorial.unity` / `Scene_Tutorial` 표기는 전부 이것으로 읽는다.
  - 이 씬은 `1st_Security_Room` 파생이라 Exe254(익스큐셔너) 트리거와 몬스터 스폰 존 등 본편 잔재물이 있다. Task 7에서 튜토리얼 구성에 맞게 정리한다.
  - `StageUpdateTrigger`는 **없음**(확인 완료). 있었다면 `demoStageDatas`(원소 1개)에 `UpdatePlayerStageIndex(1)`이 걸려 `IndexOutOfRangeException`이 났을 것이다.
  - `Init1stGameScene`이 이미 배치되어 있고 `startPosition`도 할당되어 있다. 따라서 **별도의 `InitTutorialScene` 컴포넌트를 만들지 않는다.**
- **기존 QA 도구:** `Tools/Scene Test Launcher` (단축키 **F6**)로 임의 씬을 `BootstrapInGame` 모드로 띄울 수 있다. 검증 단계에서 적극 활용한다.
- **브랜치:** 현재 `main`에 있다. 작업 시작 전 `feature/tutorial-demo` 브랜치를 생성한다.
- **네임스페이스:** 매니저류는 `namespace Managers`, 그 외 게임플레이 스크립트는 전역 네임스페이스를 쓰는 것이 이 코드베이스의 관행이다. 새 파일도 이를 따른다.

---

## File Structure

### 신규 생성

| 파일 | 책임 |
|---|---|
| `Branch/Assets/_GameAssets/01. Scripts/Game/EPlayMode.cs` | 플레이 모드 enum (`Normal` / `Demo`) |
| `Branch/Assets/_GameAssets/01. Scripts/Game/TutorialModeContext.cs` | 데모 모드 전역 정적 홀더 (배수 값) |
| `Branch/Assets/_GameAssets/01. Scripts/Data/Tutorial/TutorialBossProfile.cs` | 배수 값을 담는 ScriptableObject |
| `Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs` | **임시** 타이틀 복귀 디버그 키 (Task 6에서 제거) |
| `Branch/Assets/_GameAssets/01. Scripts/Tutorial/ITutorialCondition.cs` | 스텝 완료 조건 인터페이스 |
| `Branch/Assets/_GameAssets/01. Scripts/Tutorial/TutorialDirector.cs` | 스텝 진행 + 보스 기동 + 소프트 타임아웃 |
| `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/MoveCondition.cs` | 이동 입력 조건 |
| `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/AttackCondition.cs` | 공격 입력 조건 |
| `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/AreaEnterCondition.cs` | 트리거 볼륨 진입 조건 |
| `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/KillCountCondition.cs` | 처치 수 조건 |
| `Branch/Assets/_GameAssets/01. Scripts/GUI/UI_DemoResult.cs` | 결과 화면 컨트롤러 |
| `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity` | 튜토리얼 씬 (주소 `Scene_Tutorial`) |

### 수정

| 파일 | 변경 내용 |
|---|---|
| `01. Scripts/Managers/GameManager.cs` | `PlayMode`, `EnterPrologue(EPlayMode)`, 세션 오브젝트 레지스트리, `ReturnToTitleFromDemo()`, `EnterDemoResult()`, `GameState.DemoResult` |
| `01. Scripts/Managers/DungeonManager.cs` | `demoStageDatas`, `CurrentStageSet`, `ResetSession()`, 미니맵 인스턴스 등록, `AmonEndPhase()` 모드 분기 |
| `01. Scripts/Managers/MonsterManager.cs` | `ResetSession()` |
| `01. Scripts/Managers/GUIManager.cs` | `DemoResultUI` 슬롯 + `Update()` switch에 `DemoResult` case |
| `01. Scripts/GUI/EUIType.cs` | `DemoResult` 추가 |
| `01. Scripts/GUI/UI_Title.cs` | `OnClickDemoStart()` |
| `01. Scripts/Player/InitPlayer.cs` | 생성 오브젝트를 세션 레지스트리에 등록 |
| `01. Scripts/Monster/AI/Blackboard/Blackboard.cs` | `InitMonsterStatsByID()`에 배수 적용 |

---

## Task 0: 결함 C 런타임 확인 (진단 전용, 코드 변경 없음)

스펙 §7.1-C는 코드 정적 분석에 근거한 **추정**이다. 구현 착수 전 실제 동작을 확인해 이후 태스크의 전제를 확정한다.

**Files:** 없음 (진단만)

**Interfaces:**
- Consumes: 없음
- Produces: 결함 C의 실재 여부 판정. Task 2의 필요성을 결정한다.

- [ ] **Step 1: 본편을 실행해 인게임 진입**

Unity 에디터에서 `Branch/Assets/_GameAssets/04. Scenes/Bootstrap.unity`를 열고 Play.
타이틀에서 "게임 시작" → 프롤로그를 넘겨 인게임 진입.

- [ ] **Step 2: Hierarchy에서 오브젝트 소속 확인**

Hierarchy 창 상단의 씬 이름별 그룹을 확인한다.
`Player`, `Minimap`, `FollowCamera` 등이 **`Scene_Player`가 아니라 `Scene_Persistent` 아래에 있는지** 확인한다.

기대: `Scene_Persistent` 아래에 있다 → 결함 C 확정.

- [ ] **Step 3: 인스턴스 수 확인**

Play 중 Console에서 다음을 확인하기 위해, 임시로 Hierarchy 검색창에 `t:PlayerController`를 입력해 개수를 센다.

기대: 1개.

- [ ] **Step 4: 판정 기록**

결과를 이 계획서 하단 "Task 0 결과" 섹션에 기록한다.

- 오브젝트가 `Scene_Persistent` 아래에 있음 → **Task 2를 계획대로 수행**
- 오브젝트가 `Scene_Player` 아래에 있음 → **Task 2를 건너뛰고**, Task 3의 `DestroySessionObjects()` 호출도 제거한다. 이 경우 반복 플레이 결함은 A/B/D/E/F만 남는다.

> Task 0은 코드 변경이 없으므로 커밋하지 않는다.

---

## Task 1: 모드 플래그 + 스테이지 세트 분기 + 짧은 왕복 루프

세션 리셋(Task 2~3)을 검증하려면 "판을 끝내고 타이틀로 돌아가는" 짧은 루프가 먼저 있어야 한다. 현재 본편에는 에필로그 → 크레딧 경로밖에 없어 1회 검증에 10분 이상 걸린다. 이 태스크가 **30초짜리 왕복 루프**를 만든다.

**Files:**
- Create: `Branch/Assets/_GameAssets/01. Scripts/Game/EPlayMode.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Game/InitTutorialScene.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs`
- Create: `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/GUI/UI_Title.cs`

**Interfaces:**
- Consumes: 없음
- Produces:
  - `EPlayMode { Normal, Demo }` (전역 네임스페이스)
  - `Managers.GameManager.PlayMode` → `EPlayMode` (get 전용 프로퍼티)
  - `Managers.GameManager.EnterPrologue(EPlayMode mode)` → `void` (기존 무인자 버전 대체)
  - `Managers.GameManager.ReturnToTitleFromDemo()` → `void` (Task 2·3에서 확장)
  - `Managers.DungeonManager.CurrentStageSet` → `StageData[]` (private)

- [ ] **Step 1: 브랜치 생성**

```bash
git checkout -b feature/tutorial-demo
```

- [ ] **Step 2: `EPlayMode.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Game/EPlayMode.cs`

```csharp
/// <summary>
/// 플레이 모드. 행사 출품용 체험 플로우(Demo)와 본편(Normal)을 구분한다.
/// GameManager.EnterPrologue(EPlayMode) 시점에 결정되며, 타이틀 복귀 시 Normal로 되돌아간다.
/// </summary>
public enum EPlayMode
{
    Normal,
    Demo
}
```

- [ ] **Step 3: `GameManager`에 모드 플래그와 프롤로그 분기 추가**

`Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`

`GameState` enum 아래(`isHardMode` 필드 위)에 프로퍼티를 추가한다.

```csharp
        public EPlayMode PlayMode { get; private set; } = EPlayMode.Normal;
```

기존 `EnterPrologue()` (파일 150행 부근)를 아래로 교체한다.

```csharp
        public async void EnterPrologue(EPlayMode mode)
        {
            try
            {
                Debug.Log($"[GameManager] 게임 실행 준비 중... (mode: {mode})");
                CurrentState = GameState.Loading;

                PlayMode = mode;

                // 데모 모드는 하드 모드를 강제로 끈다.
                // 하드 모드 사망 시 RebirthGame()이 ResetCurrentStage()로 현재 스테이지를 리로드하는데,
                // 튜토리얼은 스테이지가 1개뿐이라 그것이 곧 보스전 전체 초기화를 의미한다.
                if (mode == EPlayMode.Demo) IsHardMode = false;

                // 프롤로그 재생하는 동안 플레이어 씬과 게임 씬 로드
                await DungeonManager.Instance.Init();
                await PoolManager.Instance.Init();
                await LoadPlayerScene();

                DungeonManager.Instance.SetPlayerStartPosition();

                Debug.Log("[GameManager] 게임 실행 준비 완료!");

                // 프롤로그 실행
                CurrentState = GameState.Prologue;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameManager] 게임 실행 준비 중 예외 발생: {e}");
            }
        }
```

- [ ] **Step 4: `GameManager`에 최소 버전 `ReturnToTitleFromDemo()` 추가**

`ExitGame()` 아래에 추가한다. **Task 2와 Task 3에서 이 메서드를 확장한다.**

```csharp
        /// <summary>
        /// 체험 플레이 1판을 종료하고 타이틀로 복귀한다.
        /// 행사에서는 게임을 끄지 않고 종일 반복 실행하므로, 이 경로가 세션 상태를 완전히 되돌려야 한다.
        /// (Task 2에서 세션 오브젝트 파괴, Task 3에서 매니저 상태 리셋이 추가된다.)
        /// </summary>
        public async void ReturnToTitleFromDemo()
        {
            try
            {
                Debug.Log("[GameManager] 체험 플레이 종료, 타이틀 복귀 시작...");

                // 씬 언로드
                await DungeonManager.Instance.UnloadAllStage();
                await UnloadPlayerScene();

                PlayMode = EPlayMode.Normal;

                Debug.Log("[GameManager] 타이틀 복귀 완료!");

                EnterTitle();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameManager] 타이틀 복귀 중 예외 발생: {e}");
            }
        }
```

- [ ] **Step 5: `UnloadPlayerScene()`에 null 가드 추가**

같은 파일의 `UnloadPlayerScene()`은 `MinimapObject`를 null 검사 없이 역참조한다. 반복 실행 시 NRE 원인이므로 수정한다.

```csharp
        private async Task UnloadPlayerScene()
        {
            try
            {
                // FollowCamera, MinimapObject의 FollowAudioListener 언로드 (별도의 MonoBehaviour이므로 Update 등에서 참조가 남아 있음)
                if (SoundManager.Instance.AudioListener != null)
                    SoundManager.Instance.AudioListener.GetComponent<FollowAudioListener>()?.Unload();

                if (MinimapObject != null)
                    MinimapObject.GetComponent<FollowAudioListener>()?.Unload();

                // 플레이어 씬 언로드
                await SceneController.Instance.UnloadScene("Scene_Player");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameManager] 플레이어 씬 언로드 중 예외 발생: {e}");
            }
        }
```

- [ ] **Step 6: `DungeonManager`에 스테이지 세트 분기 추가**

`Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`

`stageDatas` 필드(22행) 바로 아래에 추가한다.

```csharp
        // 체험 플레이(Demo) 전용 스테이지 데이터. 원소 1개(튜토리얼 씬)만 사용한다.
        [SerializeField] private StageData[] demoStageDatas;

        // 현재 플레이 모드에 해당하는 스테이지 세트.
        // Init / UnloadAllStage / ResetCurrentStage / UpdatePlayerStageIndex 는 모두 이 프로퍼티를 참조한다.
        private StageData[] CurrentStageSet =>
            GameManager.Instance.PlayMode == EPlayMode.Demo ? demoStageDatas : stageDatas;
```

- [ ] **Step 7: `DungeonManager`의 `stageDatas` 참조를 전부 `CurrentStageSet`으로 교체**

같은 파일에서 아래 5곳을 교체한다. `[SerializeField]` 선언부는 그대로 둔다.

| 위치 | 기존 | 변경 |
|---|---|---|
| `Init()` 루프 | `foreach (StageData stageData in stageDatas)` | `foreach (StageData stageData in CurrentStageSet)` |
| `UpdatePlayerStageIndex()` 뒤로 이동 | `await UnloadStage(stageDatas[CurrentPlayerStageIndex + 1]);` | `await UnloadStage(CurrentStageSet[CurrentPlayerStageIndex + 1]);` |
| `UpdatePlayerStageIndex()` 뒤로 이동 | `await LoadStage(stageDatas[newStageIndex - 1 < 0 ? 0 : newStageIndex - 1]);` | `await LoadStage(CurrentStageSet[newStageIndex - 1 < 0 ? 0 : newStageIndex - 1]);` |
| `UpdatePlayerStageIndex()` 앞으로 이동 | `await UnloadStage(stageDatas[CurrentPlayerStageIndex - 1]);` / `await LoadStage(stageDatas[newStageIndex + 1]);` | `CurrentStageSet[...]` |
| `UnloadAllStage()` 루프 | `foreach (StageData stageData in stageDatas)` | `foreach (StageData stageData in CurrentStageSet)` |
| `ResetCurrentStage()` | `StageData currentStageData = stageDatas[CurrentPlayerStageIndex];` | `StageData currentStageData = CurrentStageSet[CurrentPlayerStageIndex];` |

`LoadAllStage()`(테스트 전용)는 본편 전용이므로 `stageDatas`를 그대로 둔다.

- [ ] **Step 8: `InitTutorialScene.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Game/InitTutorialScene.cs`

`Init1stGameScene`과 동일한 역할이다. 별도 파일로 두는 이유는 튜토리얼 씬이 본편 1스테이지와 독립적으로 변경되기 때문이다.

```csharp
using Managers;
using UnityEngine;

/// <summary>
/// 튜토리얼 씬의 플레이어 시작 지점을 DungeonManager에 주입한다.
/// Init1stGameScene과 동일한 역할이며, 튜토리얼 씬 전용으로 분리했다.
/// </summary>
public class InitTutorialScene : MonoBehaviour
{
    [SerializeField] private GameObject startPosition;

    private void Awake()
    {
        if (!startPosition) return;

        DungeonManager.Instance.SetStartPosition(startPosition);
        DungeonManager.Instance.RestartPosition = startPosition.transform.position;
    }
}
```

- [ ] **Step 9: `DemoDebugReturn.cs` 생성 (임시)**

`Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs`

```csharp
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [임시] F9 키로 체험 플레이를 즉시 종료하고 타이틀로 복귀한다.
/// 정식 결과 화면(Task 6)이 붙기 전까지 세션 리셋을 반복 검증하기 위한 개발용 컴포넌트다.
/// Task 6 완료 시 이 파일과 씬 배치를 함께 제거한다.
/// </summary>
public class DemoDebugReturn : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.f9Key.wasPressedThisFrame) return;

        Debug.Log("[DemoDebugReturn] F9 입력 감지, 타이틀 복귀 요청");
        GameManager.Instance.ReturnToTitleFromDemo();
    }
}
```

- [ ] **Step 10: 튜토리얼 씬 생성**

Unity 에디터에서:

1. `File > New Scene` → Basic (URP) 템플릿
2. `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity`로 저장
3. 바닥 평면 생성: `GameObject > 3D Object > Plane`, Scale `(10, 1, 10)`
4. 빈 오브젝트 `PlayerStart` 생성, Position `(0, 1, 0)`
5. 빈 오브젝트 `TutorialSceneInit` 생성 → `InitTutorialScene` 컴포넌트 추가 → `startPosition`에 `PlayerStart` 할당
6. 빈 오브젝트 `DebugReturn` 생성 → `DemoDebugReturn` 컴포넌트 추가
7. 씬 저장

> NavMesh 베이크와 `arenaAnchor`는 Task 4에서 추가한다. 이 태스크는 "씬이 로드되고 플레이어가 서 있다"까지만 확인한다.

- [ ] **Step 11: Addressables 등록**

1. Project 창에서 `Tutorial.unity` 선택
2. Inspector 상단 `Addressable` 체크박스 활성화
3. Address를 **`Scene_Tutorial`** 로 변경
4. `Window > Asset Management > Addressables > Groups`에서 `GameScene` 그룹으로 드래그

- [ ] **Step 12: `DungeonManager` 프리팹에 `demoStageDatas` 설정**

`Branch/Assets/_GameAssets/02. Prefabs/Persistent/DungeonManager/DungeonManager.prefab` 열기 →
Inspector의 `Demo Stage Datas` 배열 Size를 `1`로, Element 0을 다음과 같이 설정:

- `Stage Index`: `0`
- `Stage Name`: `Scene_Tutorial`

- [ ] **Step 13: `UI_Title`에 데모 진입 메서드 추가**

`Branch/Assets/_GameAssets/01. Scripts/GUI/UI_Title.cs`

기존 `OnClickStart()`를 교체하고 데모용 메서드를 추가한다.

```csharp
    public void OnClickStart()
    {
        GameManager.Instance.EnterPrologue(EPlayMode.Normal);
    }

    /// <summary>
    /// 행사 출품용 체험 플레이 진입. 프롤로그는 본편과 공유하고, 이후 튜토리얼 씬으로 향한다.
    /// </summary>
    public void OnClickDemoStart()
    {
        GameManager.Instance.EnterPrologue(EPlayMode.Demo);
    }
```

- [ ] **Step 14: 타이틀 UI에 버튼 배치**

1. `Branch/Assets/_GameAssets/02. Prefabs/UI/UI_Title.prefab`을 Prefab 모드로 연다
2. 기존 "게임 시작" 버튼을 복제해 아래에 배치, 라벨을 **"체험 플레이"** 로 변경
4. Button의 `OnClick`에 루트 오브젝트를 할당하고 `UI_Title.OnClickDemoStart`를 선택
5. 프리팹 저장

- [ ] **Step 15: 검증 — 데모 진입**

`Bootstrap.unity`를 열고 Play → 타이틀에서 **"체험 플레이"** 클릭.

Console 기대 출력 (순서대로):
```
[GameManager] 게임 실행 준비 중... (mode: Demo)
[SceneController] Scene_Tutorial 로드 시작 (Additive)...
[SceneController] Scene_Tutorial 로드 완료!
[GameManager] 게임 실행 준비 완료!
```

프롤로그 재생 후 튜토리얼 씬의 평면 위에 플레이어가 서 있어야 한다.

- [ ] **Step 16: 검증 — 왕복 루프**

인게임 상태에서 **F9** 입력.

Console 기대 출력:
```
[DemoDebugReturn] F9 입력 감지, 타이틀 복귀 요청
[GameManager] 체험 플레이 종료, 타이틀 복귀 시작...
[SceneController] Scene_Tutorial 언로드 완료.
[SceneController] Scene_Player 언로드 완료.
[GameManager] 타이틀 복귀 완료!
```

타이틀 화면이 다시 표시되어야 한다.

> **이 시점에서 2회차 진입은 아직 실패한다.** 결함 A(`_isInit`)와 B(`LoadedStages`) 때문이며 Task 3에서 해결한다. 여기서는 "1회차 진입 → 타이틀 복귀"까지만 확인한다.

- [ ] **Step 17: 검증 — 본편 회귀**

타이틀에서 **"게임 시작"** 클릭 → 본편 1스테이지가 기존과 동일하게 로드되는지 확인.

Console 기대 출력:
```
[GameManager] 게임 실행 준비 중... (mode: Normal)
[SceneController] 1st_Security_Room 로드 시작 (Additive)...
```

- [ ] **Step 18: 커밋**

```bash
git add "Branch/Assets/_GameAssets/01. Scripts" "Branch/Assets/_GameAssets/04. Scenes" "Branch/Assets/_GameAssets/02. Prefabs" Branch/Assets/AddressableAssetsData
git commit -m "feat: 체험 플레이 모드 진입 경로와 튜토리얼 씬 스테이지 세트 분기 추가"
```

---

## Task 2: 폐기됨 — 결함 C 진단 오류

> **실행 중 판정: 결함 C는 대부분 오진이었다. 이 태스크는 수행하지 않는다.**
>
> Task 0 런타임 확인 결과, `InitPlayer`가 생성한 Player / Minimap Camera / FollowCamera는
> `Scene_Persistent`가 아니라 **`Scene_Player`에 들어간다.** Unity는 추가 로드 중인 씬의
> `Awake`에서 `Instantiate`된 오브젝트를 그 로드 중인 씬에 배치하기 때문이다.
> 2회차 진입 후 `t:PlayerController`는 1개로 확인되었다.
>
> **다만 미니맵 1건은 실제 누수가 맞다.** `DungeonManager.LoadMiniMap()`은 씬 로드 중이
> 아니라 `Init()` 흐름에서 호출되므로 Active Scene(`Scene_Persistent`)에 생성된다.
> 현재는 결함 A(`_isInit` 미복구)가 2회차 `Init()`을 통째로 막고 있어 누수가 드러나지
> 않을 뿐이며, **Task 3에서 `_isInit`을 고치는 순간 판마다 하나씩 쌓인다.**
>
> → 범용 레지스트리 없이, `DungeonManager`가 인스턴스를 필드로 들고
> `ResetSession()`에서 직접 파괴하는 방식으로 **Task 3에 흡수**했다.

### 실행 중 추가로 발견해 해결한 것 (Task 3에 포함)

| 증상 | 원인 | 해결 |
|---|---|---|
| F9 복귀 후 타이틀 버튼 클릭 불가 | `EnterTitle()`이 커서 상태를 건드리지 않아 인게임의 `CursorLockMode.Locked`가 유지됨 | `EnterTitle()` 진입 시 커서 해제. 크레딧 → 타이틀 경로도 같은 버그였으므로 함께 해결됨 |
| 2회차 프롤로그가 검은 화면 | `UI_Prologue._isEnd`가 `true`로 남아 `Update()`가 즉시 return, `fadePanel`은 알파 1 | `ReturnToTitleFromDemo()`에서 `Scene_UI`를 언로드 → `EnterTitle()`이 재로드 |
| 타이틀 인트로 애니메이션 미재생 | `TitleLogoDT`의 `isLogoAnimaEnd` 등이 `true`로 남아 `Update()` early return | 위와 동일 (UI 씬 재생성) |
| 언로드 중 `Player` 참조 예외 위험 | `await` 중에도 `Update()`가 돌아 `PlayingProcess()`가 파괴된 `Player`를 참조 | `ReturnToTitleFromDemo()` 맨 앞에서 `CurrentState = Loading` |

UI 씬 재로드를 택한 이유: 개별 UI 스크립트에 리셋을 넣으면 아직 발견하지 못한 같은 종류의
버그(다른 `Start()` 기반 UI 초기화)가 남는다. 씬을 통째로 다시 만들면 `InitUI.Awake()`가
UI 프리팹을 새로 인스턴스화하므로 모든 UI가 1회차와 동일한 상태에서 시작한다.
본편 `ExitGame()`이 이미 쓰는 경로라 새로 검증할 코드도 아니다.

---

## Task 2 (원안, 참고용): 세션 오브젝트 레지스트리 (결함 C)

> **위 판정에 따라 수행하지 않는다.** 이후 유사 증상이 나타날 때의 참고 자료로만 남긴다.

Active Scene이 `Scene_Persistent`로 고정되어 있어, 부모 없이 `Instantiate`된 오브젝트는 전부 Persistent에 생성되고 씬 언로드로 정리되지 않는다. 씬 소속을 건드리지 않고 명시적 레지스트리로 해결한다.

**Files:**
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Player/InitPlayer.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`

**Interfaces:**
- Consumes: `Managers.GameManager.ReturnToTitleFromDemo()` (Task 1)
- Produces:
  - `Managers.GameManager.RegisterSessionObject(GameObject go)` → `void`
  - `Managers.GameManager.DestroySessionObjects()` → `void` (private)

- [ ] **Step 1: `GameManager`에 레지스트리 추가**

`Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`

파일 상단 `using`에 `System.Collections.Generic`이 없으면 추가한다.

```csharp
using System.Collections.Generic;
```

`_rebirthRoutine` 필드 아래에 추가한다.

```csharp
        // 판(session) 단위로 생성되어 타이틀 복귀 시 파괴되어야 할 오브젝트 목록.
        // Active Scene이 Scene_Persistent로 고정되어 있어(라이팅 베이크/컬링 유지 목적)
        // 부모 없이 Instantiate된 오브젝트는 씬 언로드로 정리되지 않는다.
        // 따라서 생성 측이 직접 등록하고 ReturnToTitleFromDemo()가 일괄 파괴한다.
        private readonly List<GameObject> _sessionObjects = new();

        public void RegisterSessionObject(GameObject go)
        {
            if (go != null) _sessionObjects.Add(go);
        }

        private void DestroySessionObjects()
        {
            foreach (GameObject go in _sessionObjects)
            {
                if (go != null) Destroy(go);
            }

            _sessionObjects.Clear();
            Debug.Log("[GameManager] 세션 오브젝트 정리 완료");
        }
```

- [ ] **Step 2: `ReturnToTitleFromDemo()`에 파괴 호출 추가**

`UnloadPlayerScene()` 직후, `PlayMode` 복구 직전에 삽입한다. **순서가 중요하다** — `UnloadPlayerScene()`이 `FollowAudioListener.Unload()`를 호출하므로 파괴는 그 이후여야 한다.

```csharp
                // 씬 언로드
                await DungeonManager.Instance.UnloadAllStage();
                await UnloadPlayerScene();

                // 세션 오브젝트 일괄 파괴 (씬 언로드 "이후")
                DestroySessionObjects();

                Player = null;
                FollowCamera = null;
                MinimapObject = null;
                PlayMode = EPlayMode.Normal;
```

- [ ] **Step 3: `InitPlayer`에서 생성 오브젝트 등록**

`Branch/Assets/_GameAssets/01. Scripts/Player/InitPlayer.cs`

`Awake()`의 `Instantiate` 루프에 등록 한 줄을 추가한다.

```csharp
            foreach (PlayerInfo obj in initializePlayerPrefabs)
            {
                if (obj.prefab is null) continue;
                if (createdObj.ContainsKey(obj.prefabType)) continue;

                GameObject objInstance = Instantiate(obj.prefab);
                objInstance.SetActive(obj.isActive);

                // Active Scene이 Scene_Persistent로 고정되어 있어 이 오브젝트들은
                // Scene_Player 언로드로 정리되지 않는다. 세션 레지스트리에 등록해 명시적으로 파괴한다.
                GameManager.Instance.RegisterSessionObject(objInstance);

                createdObj.Add(obj.prefabType, objInstance);
            }
```

- [ ] **Step 4: `DungeonManager`에서 미니맵 등록**

`Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`의 `LoadMiniMap()`을 교체한다.

```csharp
        /// <summary>
        /// 미니맵 리소스 로드
        /// </summary>
        private void LoadMiniMap()
        {
            if (miniMapPrefab != null)
            {
                GameObject miniMap = Instantiate(miniMapPrefab);

                // Active Scene(Scene_Persistent)에 생성되므로 씬 언로드로 정리되지 않는다.
                GameManager.Instance.RegisterSessionObject(miniMap);
            }
            else
            {
                Debug.LogWarning("[DungeonManager] 미니맵 프리팹이 할당되지 않았습니다.");
            }
        }
```

- [ ] **Step 5: 검증 — 세션 오브젝트가 파괴되는지 확인**

`Bootstrap.unity` Play → "체험 플레이" → 인게임 진입.

Hierarchy 검색창에 `t:PlayerController` 입력 → **1개** 확인.

**F9** 입력 후 타이틀 복귀.

Console 기대 출력에 다음이 포함되어야 한다:
```
[GameManager] 세션 오브젝트 정리 완료
```

타이틀 상태에서 Hierarchy 검색창에 `t:PlayerController` 입력 → **0개**여야 한다.
`Scene_Persistent` 아래에 `Minimap`, `FollowCamera` 잔존물이 없어야 한다.

- [ ] **Step 6: 커밋**

```bash
git add "Branch/Assets/_GameAssets/01. Scripts"
git commit -m "fix: 세션 오브젝트 레지스트리 추가로 Persistent 씬 누적 해소"
```

---

## Task 3: 매니저 세션 리셋 (결함 A·B·D·E·F)

2회차 진입을 막는 매니저 상태를 되돌린다. **이 태스크 완료 후에야 반복 플레이가 가능해지고, 이후 모든 태스크의 QA가 성립한다.**

**Files:**
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/MonsterManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`

**Interfaces:**
- Consumes: `Managers.GameManager.ReturnToTitleFromDemo()` (Task 1), `DestroySessionObjects()` (Task 2)
- Produces:
  - `Managers.DungeonManager.ResetSession()` → `void`
  - `Managers.MonsterManager.ResetSession()` → `void`

- [ ] **Step 1: `DungeonManager.ResetSession()` 추가 (결함 A·B)**

`Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`

`Init()` 아래에 추가한다.

```csharp
        /// <summary>
        /// 한 판이 끝났을 때 스테이지 로드 상태를 초기 상태로 되돌린다.
        /// _isInit이 복구되지 않으면 2회차 Init()이 즉시 return하여 스테이지가 아예 로드되지 않고,
        /// LoadedStages가 비워지지 않으면 LoadStage()가 "이미 로드됨"으로 판단해 스킵한다.
        /// </summary>
        public void ResetSession()
        {
            _isInit = false;
            LoadedStages.Clear();
            CurrentPlayerStageIndex = 0;

            amonFirstPhase = null;
            amonSecondPhasePrefab = null;
            playerTeleportPoint = null;
            playerRespawnPoint = null;
            startPosition = null;

            Debug.Log("[DungeonManager] 세션 리셋 완료");
        }
```

> 보스/좌표 참조를 null로 되돌리는 이유: 이들은 `BossTrigger`(본편) 또는 `TutorialDirector`(데모)가 **씬에서** 주입한다. 씬이 언로드되면 파괴된 오브젝트를 가리키게 되므로 반드시 해제한다.

- [ ] **Step 2: `MonsterManager.ResetSession()` 추가 (결함 F)**

`Branch/Assets/_GameAssets/01. Scripts/Managers/MonsterManager.cs`

`ReleaseAllMonsters()` 아래에 추가한다.

```csharp
        /// <summary>
        /// 한 판이 끝났을 때 몬스터 참조를 정리한다.
        /// 씬 언로드 "전에" 호출해야 대여 중인 풀 오브젝트가 정상 회수된다.
        /// </summary>
        public void ResetSession()
        {
            ReleaseAllMonsters();
            Player = null;

            Debug.Log("[MonsterManager] 세션 리셋 완료");
        }
```

- [ ] **Step 3: `ReturnToTitleFromDemo()` 완성 (결함 D·E 포함)**

`Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`의 `ReturnToTitleFromDemo()`를 아래로 교체한다.

```csharp
        /// <summary>
        /// 체험 플레이 1판을 종료하고 타이틀로 복귀한다.
        /// 행사에서는 게임을 끄지 않고 종일 반복 실행하므로, 이 경로가 세션 상태를 완전히 되돌려야 한다.
        ///
        /// 호출 순서에 의존성이 있다.
        ///  1) 풀 오브젝트 회수는 씬 언로드 "전에" (씬과 함께 파괴되면 풀 밖에서 사라진다)
        ///  2) 세션 오브젝트 파괴는 씬 언로드 "이후에" (UnloadPlayerScene이 FollowAudioListener를 정리한다)
        /// </summary>
        public async void ReturnToTitleFromDemo()
        {
            try
            {
                Debug.Log("[GameManager] 체험 플레이 종료, 타이틀 복귀 시작...");

                // 1. 씬 언로드 "전에" 풀 오브젝트 회수
                MonsterManager.Instance.ResetSession();

                // 2. 진행 중 부활 코루틴 중단
                if (_rebirthRoutine != null)
                {
                    StopCoroutine(_rebirthRoutine);
                    _rebirthRoutine = null;
                }

                // 3. 씬 언로드
                await DungeonManager.Instance.UnloadAllStage();
                await UnloadPlayerScene();

                // 4. 세션 오브젝트 일괄 파괴 (씬 언로드 "이후")
                DestroySessionObjects();

                // 5. 매니저 상태 리셋
                DungeonManager.Instance.ResetSession();
                DungeonStateManager.Instance.ClearStates();

                // 6. 참조 및 모드 복구
                Player = null;
                FollowCamera = null;
                MinimapObject = null;
                PlayMode = EPlayMode.Normal;

                Debug.Log("[GameManager] 타이틀 복귀 완료!");

                // 7. 타이틀 진입
                //    Scene_UI는 언로드하지 않는다. EnterTitle()이 호출하는 LoadGUI()는
                //    SceneController의 중복 로드 방지에 의해 no-op이 되며,
                //    GUIManager가 초기화된 상태를 유지하는 편이 안전하다.
                EnterTitle();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameManager] 타이틀 복귀 중 예외 발생: {e}");
            }
        }
```

- [ ] **Step 4: 검증 — 3회 왕복**

`Bootstrap.unity` Play → 아래를 **3회 반복**한다.

1. 타이틀에서 "체험 플레이" 클릭
2. 프롤로그 넘기기
3. 튜토리얼 씬에 플레이어가 서 있는지 확인
4. **F9** 입력 → 타이틀 복귀

**매회** Console에 다음이 출력되어야 한다:
```
[GameManager] 게임 실행 준비 중... (mode: Demo)
[SceneController] Scene_Tutorial 로드 시작 (Additive)...
[SceneController] Scene_Tutorial 로드 완료!
...
[MonsterManager] 세션 리셋 완료
[SceneController] Scene_Tutorial 언로드 완료.
[SceneController] Scene_Player 언로드 완료.
[GameManager] 세션 오브젝트 정리 완료
[DungeonManager] 세션 리셋 완료
[GameManager] 타이틀 복귀 완료!
```

**실패 판정 기준**: 2회차 이후 `Scene_Tutorial 로드 시작` 로그가 나오지 않으면 결함 A/B가 남아 있는 것이다.

- [ ] **Step 5: 검증 — 오브젝트 누적 없음**

3회 왕복 후 타이틀 상태에서 Hierarchy 검색:

- `t:PlayerController` → **0개**
- `Scene_Persistent` 아래에 `Minimap` 잔존물 없음

- [ ] **Step 6: 검증 — 본편 회귀**

3회 왕복 직후 "게임 시작"으로 본편에 진입해 1스테이지가 정상 로드되는지 확인한다.
`CurrentStageSet`이 `stageDatas`로 되돌아왔는지(`1st_Security_Room` 로드 로그) 확인한다.

- [ ] **Step 7: 커밋**

```bash
git add "Branch/Assets/_GameAssets/01. Scripts"
git commit -m "fix: 매니저 세션 리셋 추가로 반복 플레이 시 스테이지 미로드 문제 해소"
```

---

## Task 4: 아몬 2페이즈 단독 기동

스펙 §5의 의존성 5개를 충족시켜 튜토리얼 씬에서 아몬 2페이즈를 1페이즈 없이 동작시킨다.

**Files:**
- Modify: `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`

**Interfaces:**
- Consumes: `Managers.DungeonManager.AmonSecondPhasePrefab` / `.PlayerTeleportPoint` / `.PlayerRespawnPoint` (기존 setter)
- Produces: 튜토리얼 씬에 배치된 `AmonPaseTwoFSM` 인스턴스 (Task 7의 `TutorialDirector`가 참조)

- [ ] **Step 1: 참조 씬에서 아몬 2페이즈 구성 확인**

`Branch/Assets/_GameAssets/04. Scenes/GameScene/8th_Another_Space.unity`를 열고 아몬 2페이즈 오브젝트 계층을 확인한다.
`Blackboard` 컴포넌트의 `amonBody` / `amonShield` / `amonEnergyBall` / `amonDeathModel` / `arenaAnchor` 할당 상태를 기록해둔다. 튜토리얼 씬에서 동일하게 구성해야 한다.

- [ ] **Step 2: 튜토리얼 씬에 아몬 2페이즈 배치**

`Tutorial.unity`를 열고:

1. `8th_Another_Space`의 아몬 2페이즈 루트 오브젝트를 복사해 `Tutorial.unity`에 붙여넣는다
2. `AmonPaseTwoFSM` 컴포넌트의 `isEnabled`를 **체크 해제**한다
3. Position을 `(0, 1, 15)`로 설정 (플레이어 시작 지점에서 15m 앞)

- [ ] **Step 3: 아레나 앵커 배치**

1. 빈 오브젝트 `ArenaAnchor` 생성, Position `(0, 0, 15)`
2. 아몬의 `Blackboard` 컴포넌트 → `arenaAnchor` 필드에 `ArenaAnchor` 할당

> 미할당 시 스킬의 워프/스폰 좌표가 월드 원점 기준으로 폴백되어 엉뚱한 위치에서 발동한다.

- [ ] **Step 4: 텔레포트/리스폰 지점 배치**

1. 빈 오브젝트 `PlayerTeleportPoint` 생성, Position `(0, 1, 5)`
2. 빈 오브젝트 `PlayerRespawnPoint` 생성, Position `(0, 1, 0)`

- [ ] **Step 5: NavMesh 확인 (조사 결과에 따라 수정됨)**

> **정정**: 원안의 "씬에 NavMesh를 베이크한다"는 이 프로젝트의 구조와 맞지 않는다.
> 조사 결과 **게임 씬 10개 전부 `m_NavMeshData: {fileID: 0}`으로 베이크된 NavMesh가 없다.**
> 대신 `02. Prefabs/Persistent/MapManager.prefab`이 `NavMeshSurface` 컴포넌트를 갖고 있고,
> 그 `m_NavMeshData`가 `04. Scenes/GameScene/9th_Warpgate_.../NavMesh-MapManager.asset`을
> 가리킨다. `InitPersistent`가 MapManager를 생성하면 이 NavMesh가 런타임에 월드에 추가된다.
> `m_CollectObjects: 0`(All)이므로 베이크 당시 열려 있던 모든 씬 지오메트리가 한 덩어리다.

먼저 **추가 작업이 필요한지부터 확인한다.** `0th_Demo_Room`은 `1st_Security_Room`의 복제본이므로,
월드 좌표를 그대로 두었다면 기존 `NavMesh-MapManager.asset`이 이미 덮고 있을 수 있다.

Step 8의 F6 테스트에서 **아몬이 걸어서 추격하는지** 확인한다.

- 추격한다 → NavMesh 작업 불필요. 이 Step 종료.
- 추격하지 않는다 → 아래 전용 Surface를 추가한다.

**전용 NavMeshSurface 추가 (필요할 때만)**

1. `0th_Demo_Room.unity`만 단독으로 연다 (다른 씬이 열려 있으면 함께 구워진다)
2. 빈 오브젝트 `NavMeshSurface_DemoRoom` 생성
3. `Add Component > Navigation > NavMeshSurface`
4. `Collect Objects`를 `All`로 두고 `Bake` 클릭
5. 씬 이름 폴더에 `NavMesh-NavMeshSurface_DemoRoom.asset`이 생성되는지 확인

> **`NavMesh-MapManager.asset`을 다시 굽지 않는다.** 그것은 본편 전체가 공유하는 에셋이라,
> 모든 스테이지 씬을 정확히 같은 조합으로 열지 않은 상태에서 재베이크하면 본편 NavMesh가
> 축소되어 회귀가 발생한다. 데모룸 전용 Surface를 두면 씬과 함께 로드/언로드되어
> 반복 플레이에도 깔끔하다.

> **참고**: `01. Scripts/Editor/LoadAllGameScene.cs`의 경로 상수가
> `Assets/_Test/Scene/GameScene`으로 되어 있는데 실제 씬은
> `Assets/_GameAssets/04. Scenes/GameScene`으로 이동했다. 이 툴은 현재 동작하지 않는다.
> 이번 범위 밖이지만 NavMesh 재베이크가 필요해지면 먼저 고쳐야 한다.

- [ ] **Step 6: 임시 기동 컴포넌트 배치**

Task 7의 `TutorialDirector`가 완성되기 전까지, 기존 테스트 컴포넌트로 기동을 검증한다.

1. 빈 오브젝트 `BossActivator` 생성
2. `SceneTestFSMActivator` 컴포넌트 추가
3. `fsmToEnable` 배열에 아몬 2페이즈의 `AmonPaseTwoFSM` 할당

- [ ] **Step 7: `AmonEndPhase()`에 모드 분기 추가**

`Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`

이 시점에는 `EnterDemoResult()`가 아직 없으므로 **임시로 `ReturnToTitleFromDemo()`를 호출**한다. Task 6에서 `EnterDemoResult()`로 교체한다.

```csharp
        // 아몬 2페이즈 종료
        public void AmonEndPhase()
        {
            if (amonSecondPhasePrefab != null)
                amonSecondPhasePrefab.isEnabled = false;

            // 체험 플레이에서는 리스폰 대신 결과 화면으로 진입한다.
            if (GameManager.Instance.PlayMode == EPlayMode.Demo)
            {
                // TODO(Task 6): EnterDemoResult() 로 교체
                GameManager.Instance.ReturnToTitleFromDemo();
                return;
            }

            GameObject playerObj = MonsterManager.Instance.Player;
            if (playerObj == null || playerRespawnPoint == null) return;

            CharacterController controller = playerObj.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            playerObj.transform.position = playerRespawnPoint.position;
            if (controller != null) controller.enabled = true;
        }
```

- [ ] **Step 8: 검증 — Scene Test Launcher로 단독 기동**

1. `Tools > Scene Test Launcher` 열기
2. `Target Scene`에 `Tutorial.unity` 지정
3. `Load Mode`를 `BootstrapInGame`으로 설정
4. `Enemy Damage Multiplier`를 `50`으로 설정 (빠른 처치 확인용)
5. Play

Console 기대 출력:
```
[SceneTools] BootstrapInGame 셋업 시작: Scene_Tutorial
[SceneTools] FSM 활성화: (아몬 오브젝트 이름)
Amon Pase Two has spawned.
```

- [ ] **Step 9: 검증 — 스펙 §5 체크리스트 5항목**

| # | 확인 항목 | 기대 결과 |
|---|---|---|
| 1 | 타겟 인식 | 등장 연출(5초) 후 아몬이 플레이어 방향으로 이동/공격 |
| 2 | `arenaAnchor` | 텔레포트/소환 스킬이 아레나 근처에서 발동 (원점 근처가 아님) |
| 3 | `AmonEndPhase()` | 처치 후 5초 뒤 로그 `Amon Pase Two has died.` → 예외 없이 진행 |
| 4 | 보스 HP 바 | 피격 시 화면 상단에 "해방된 아몬" HP 바 표시 |
| 5 | NavMesh | 거리를 벌리면 아몬이 걸어서 추격 |

- [ ] **Step 10: 검증 — 정식 경로로 기동**

`Bootstrap.unity` Play → "체험 플레이" → 프롤로그 → 튜토리얼 씬.

`SceneTestFSMActivator`는 `ISceneTestHook`이라 정식 경로에서는 호출되지 않는다.
따라서 이 단계에서는 **Play 중 Inspector에서 `AmonPaseTwoFSM.isEnabled`를 수동으로 체크**해 기동시킨다.

처치 후 `AmonEndPhase()` → `ReturnToTitleFromDemo()`가 실행되어 타이틀로 복귀하는지 확인한다.

- [ ] **Step 11: 커밋**

```bash
git add "Branch/Assets/_GameAssets/01. Scripts" "Branch/Assets/_GameAssets/04. Scenes"
git commit -m "feat: 튜토리얼 씬에 아몬 2페이즈 단독 기동 구성 추가"
```

---

## Task 5: 보스 스탯 배수 오버라이드

구글 시트를 수정하지 않고 런타임에 보스 스펙을 낮춘다.

**Files:**
- Create: `Branch/Assets/_GameAssets/01. Scripts/Game/TutorialModeContext.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Data/Tutorial/TutorialBossProfile.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Monster/AI/Blackboard/Blackboard.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`

**Interfaces:**
- Consumes: `Managers.GameManager.PlayMode` (Task 1)
- Produces:
  - `TutorialModeContext.IsActive` → `bool` (static field)
  - `TutorialModeContext.BossHealthMultiplier` → `float` (static field)
  - `TutorialModeContext.BossDamageMultiplier` → `float` (static field)
  - `TutorialModeContext.Apply(TutorialBossProfile profile)` → `void`
  - `TutorialModeContext.Reset()` → `void`
  - `TutorialBossProfile.healthMultiplier` / `.damageMultiplier` → `float`

- [ ] **Step 1: `TutorialModeContext.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Game/TutorialModeContext.cs`

```csharp
using UnityEngine;

/// <summary>
/// 체험 플레이(Demo) 전용 전역 설정 홀더. 본편에서는 기본값을 유지하므로 영향이 없다.
/// TestManager와 동일한 정적 클래스 패턴이며, 스탯 초기화 핫패스에서 싱글톤 조회 없이 바로 읽는다.
///
/// 값은 Blackboard.InitMonsterStatsByID()가 시트에서 스탯을 채운 "직후"에 적용된다.
/// Init()이 여러 번 호출되어도 매번 시트에서 원본을 새로 읽으므로 배수가 중첩되지 않는다.
/// </summary>
public static class TutorialModeContext
{
    public static bool IsActive;
    public static float BossHealthMultiplier = 1f;
    public static float BossDamageMultiplier = 1f;

    public static void Apply(TutorialBossProfile profile)
    {
        if (profile == null)
        {
            Debug.LogWarning("[TutorialModeContext] 프로필이 null이라 기본 배수(1.0)를 사용합니다.");
            Reset();
            IsActive = true;
            return;
        }

        IsActive = true;
        BossHealthMultiplier = profile.healthMultiplier;
        BossDamageMultiplier = profile.damageMultiplier;

        Debug.Log($"[TutorialModeContext] 배수 적용 - HP x{BossHealthMultiplier}, DMG x{BossDamageMultiplier}");
    }

    public static void Reset()
    {
        IsActive = false;
        BossHealthMultiplier = 1f;
        BossDamageMultiplier = 1f;
    }
}
```

- [ ] **Step 2: `TutorialBossProfile.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Data/Tutorial/TutorialBossProfile.cs`

```csharp
using UnityEngine;

/// <summary>
/// 체험 플레이용 몬스터 스탯 배수. 구글 시트를 수정하지 않고 런타임에만 적용한다.
/// 밸런싱은 이 에셋의 값만 바꿔 조절한다.
/// </summary>
[CreateAssetMenu(fileName = "TutorialBossProfile", menuName = "Scriptable Object/Tutorial Boss Profile", order = 22)]
public class TutorialBossProfile : ScriptableObject
{
    [Tooltip("최대 체력 배수. 1보다 작으면 약해진다.")]
    [Range(0.05f, 1f)] public float healthMultiplier = 0.4f;

    [Tooltip("공격력 배수. 1보다 작으면 약해진다.")]
    [Range(0.05f, 1f)] public float damageMultiplier = 0.6f;
}
```

- [ ] **Step 3: `Blackboard`에 배수 적용**

`Branch/Assets/_GameAssets/01. Scripts/Monster/AI/Blackboard/Blackboard.cs`

`InitMonsterStatsByID()`의 "몬스터 스탯 초기화" 블록 **직후**, "몬스터 스킬 초기화" 블록 **직전**에 삽입한다.

```csharp
            // 체험 플레이(Demo) 전용 스탯 배수.
            // 시트에서 원본 값을 채운 직후에 적용하므로, Init()이 여러 번 호출되어도 중첩되지 않는다.
            if (TutorialModeContext.IsActive)
            {
                float scaledMaxHp = (float)_map["maxHealth"] * TutorialModeContext.BossHealthMultiplier;
                _map["maxHealth"] = scaledMaxHp;
                _map["health"] = scaledMaxHp;
                _map["damage"] = (float)_map["damage"] * TutorialModeContext.BossDamageMultiplier;
            }
```

> `CurrentHealth = MaxHealth`는 호출자인 `Blackboard.Init()`에서 이 메서드 **이후에** 실행되므로 별도 처리가 필요 없다.

- [ ] **Step 4: `GameManager`에서 컨텍스트 리셋 연결**

`ReturnToTitleFromDemo()`의 "5. 매니저 상태 리셋" 블록에 한 줄을 추가한다.

```csharp
                // 5. 매니저 상태 리셋
                DungeonManager.Instance.ResetSession();
                DungeonStateManager.Instance.ClearStates();
                TutorialModeContext.Reset();
```

- [ ] **Step 5: 프로필 에셋 생성**

Project 창에서 `Branch/Assets/Resources/Tutorial/` 우클릭 →
`Create > Scriptable Object > Tutorial Boss Profile` →
이름을 `TutorialBossProfile`로 지정.

값은 기본값(`healthMultiplier = 0.4`, `damageMultiplier = 0.6`)으로 두고 Task 8에서 실측 조정한다.

- [ ] **Step 6: 임시 적용 지점 배치**

`TutorialDirector`(Task 7)가 프로필을 주입하기 전까지, 임시 컴포넌트로 검증한다.

`Tutorial.unity`의 `BossActivator` 오브젝트에 아래 스크립트를 임시로 추가한다.

`Branch/Assets/_GameAssets/01. Scripts/Test/TutorialProfileApplier.cs`

```csharp
using UnityEngine;

/// <summary>
/// [임시] TutorialDirector(Task 7)가 완성되기 전까지 프로필을 수동 주입한다.
/// Task 7 완료 시 이 파일과 씬 배치를 함께 제거한다.
/// </summary>
public class TutorialProfileApplier : MonoBehaviour
{
    [SerializeField] private TutorialBossProfile profile;

    private void Awake()
    {
        TutorialModeContext.Apply(profile);
    }
}
```

`profile` 필드에 Step 5에서 만든 에셋을 할당한다.

> **`Awake()`에서 적용하는 이유와 순서 근거**
>
> Unity는 동일 씬 내 컴포넌트의 `Awake` 순서를 보장하지 않으므로, 아몬 오브젝트의
> `Awake` / `OnEnable`이 이 컴포넌트의 `Awake`보다 먼저 실행될 수 있다.
> 그 경우 첫 `Blackboard.Init()`은 배수 없이(x1.0) 실행된다.
>
> 그럼에도 안전한 이유는 `FSM.Start()`가 `Init()`을 **무조건 다시 호출**하기 때문이다.
> `Start()`는 해당 씬의 모든 `Awake`가 끝난 뒤 실행되므로 주입이 반드시 앞서고,
> `InitMonsterStatsByID()`가 매번 시트에서 원본을 새로 읽으므로 최종 값은 항상 올바르다.
>
> **`FSM.Start()`의 무조건 `Init()` 호출을 조건부로 바꾸지 말 것.** 이 성질이 깨진다.
> Step 7의 검증은 이 순서가 실제로 성립하는지를 확인하는 것이기도 하다.

- [ ] **Step 7: 검증 — 배수 반영**

`Tools > Scene Test Launcher`에서 `Tutorial.unity`를 `BootstrapInGame`으로 실행.
`Enemy Damage Multiplier`는 `1`로 되돌린다.

Console 기대 출력:
```
[TutorialModeContext] 배수 적용 - HP x0.4, DMG x0.6
```

아몬 기동 후 `AmonPaseTwoFSM.Think()`가 매 프레임 출력하는
`{CurrentHealth} / {MaxHealth}` 로그의 `MaxHealth`가 **시트 원본의 0.4배**인지 확인한다.

시트 원본 값은 `DataManager`의 `SheetData` 에셋에서 `CharacterStats` 시트의 해당 `id` 행 `maxHp`로 확인한다.

- [ ] **Step 8: 검증 — 중첩 미발생**

Play 중 아몬 오브젝트를 비활성화했다가 다시 활성화한다(`FSM.OnEnable()` → `Init()` 재호출).

`MaxHealth` 로그 값이 **변하지 않아야 한다.** 0.16배(0.4²)로 줄어들면 적용 지점이 잘못된 것이다.

- [ ] **Step 9: 검증 — 본편 미영향**

`Bootstrap.unity` Play → "게임 시작"(본편) → 1스테이지 몬스터와 교전.

Console에 `[TutorialModeContext] 배수 적용` 로그가 **출력되지 않아야** 하고,
몬스터 체력이 기존과 동일해야 한다.

- [ ] **Step 10: 커밋**

```bash
git add "Branch/Assets/_GameAssets/01. Scripts" "Branch/Assets/_GameAssets/04. Scenes" Branch/Assets/Resources
git commit -m "feat: 체험 플레이용 보스 스탯 런타임 배수 오버라이드 추가"
```

---

## Task 6: 결과 화면 + 정식 타이틀 복귀

임시 F9 디버그 복귀를 정식 결과 화면으로 교체한다.

**Files:**
- Modify: `Branch/Assets/_GameAssets/01. Scripts/GUI/EUIType.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/GUIManager.cs`
- Modify: `Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/GUI/UI_DemoResult.cs`
- Delete: `Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs`
- Modify: `Branch/Assets/_GameAssets/04. Scenes/UI_Global.unity` (결과 패널 프리팹 등록)
- Modify: `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity` (디버그 오브젝트 제거)

**Interfaces:**
- Consumes: `Managers.GameManager.ReturnToTitleFromDemo()` (Task 3)
- Produces:
  - `Managers.GameManager.GameState.DemoResult` (enum 멤버)
  - `Managers.GameManager.EnterDemoResult()` → `void`
  - `UI.EUIType.DemoResult` (enum 멤버)

- [ ] **Step 1: `EUIType`에 멤버 추가**

`Branch/Assets/_GameAssets/01. Scripts/GUI/EUIType.cs`

```csharp
    public enum EUIType
    {
        None = 0,
        Title,
        Prologue,
        GameUIController,
        Epilogue,
        Loading,
        Credit,
        DemoResult
    }
```

- [ ] **Step 2: `GameState`에 멤버 추가**

`Branch/Assets/_GameAssets/01. Scripts/Managers/GameManager.cs`

```csharp
        public enum GameState
        {
            Loading,
            Title,
            Prologue,
            Epilogue,
            Playing,
            Paused,
            GameOver,
            Credit,
            DemoResult
        }
```

- [ ] **Step 3: `EnterDemoResult()` 추가**

`ReturnToTitleFromDemo()` 위에 추가한다.

```csharp
        /// <summary>
        /// 체험 플레이 클리어 시 결과 화면으로 진입한다.
        /// 플레이어/몬스터를 정지시켜 결과 화면 뒤에서 전투가 계속되지 않게 한다.
        /// </summary>
        public void EnterDemoResult()
        {
            try
            {
                Debug.Log("[GameManager] 체험 플레이 결과 화면 진입");

                PauseObjects();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                CurrentState = GameState.DemoResult;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameManager] 결과 화면 진입 중 예외 발생: {e}");
            }
        }
```

> `PauseObjects()`가 `CurrentState = GameState.Paused`로 덮어쓰므로, **그 이후에** `DemoResult`를 대입하는 순서를 지킨다.

- [ ] **Step 4: `GUIManager`에 슬롯과 상태 분기 추가**

`Branch/Assets/_GameAssets/01. Scripts/Managers/GUIManager.cs`

프로퍼티 선언부에 추가한다.

```csharp
        private GameObject DemoResultUI { get; set; }
```

`Init()`의 `uiInstances` 조회 블록에 추가한다.

```csharp
                if (uiInstances.TryGetValue(EUIType.DemoResult, out GameObject demoResultUI) && demoResultUI != null)
                {
                    DemoResultUI = demoResultUI;
                }
```

`CheckValidation` 호출 목록에 추가한다.

```csharp
                CheckValidation(EUIType.DemoResult, demoResultUI);
```

`UnloadGUI()`의 참조 해제 목록에 추가한다.

```csharp
                DemoResultUI = null;
```

`Update()`의 기존 6개 case 각각에 `DemoResultUI.SetActive(false);` 한 줄을 추가하고, 새 case를 추가한다.

```csharp
                case GameManager.GameState.DemoResult:
                    TitleUI.SetActive(false);
                    PrologueUI.SetActive(false);
                    GameUIController.gameObject.SetActive(false);
                    EpilogueUI.SetActive(false);
                    LoadingUI.SetActive(false);
                    CreditUI.SetActive(false);
                    DemoResultUI.SetActive(true);
                    break;
```

- [ ] **Step 5: `UI_DemoResult.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/GUI/UI_DemoResult.cs`

```csharp
using Managers;
using TMPro;
using UnityEngine;

/// <summary>
/// 체험 플레이 결과 화면. 감사 문구를 표시하고 타이틀로 복귀시킨다.
/// </summary>
public class UI_DemoResult : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI buttonLabel;

    private const string KoMessage = "체험 플레이를 즐겨주셔서 감사합니다!";
    private const string EnMessage = "Thank you for playing the demo!";
    private const string KoButton = "타이틀로";
    private const string EnButton = "Back to Title";

    private void OnEnable()
    {
        bool isKorean = LocalizationManager.IsKorean;

        if (messageText != null) messageText.text = isKorean ? KoMessage : EnMessage;
        if (buttonLabel != null) buttonLabel.text = isKorean ? KoButton : EnButton;
    }

    public void OnClickReturnToTitle()
    {
        GameManager.Instance.ReturnToTitleFromDemo();
    }
}
```

- [ ] **Step 6: 결과 패널 프리팹 제작**

1. `Branch/Assets/_GameAssets/02. Prefabs/UI/UI_Epilogue_New.prefab`을 복제해 같은 폴더에 `UI_DemoResult.prefab`으로 이름 변경
2. 내용을 비우고 다음만 남긴다: 전체 화면 반투명 검정 `Image`, 중앙 `TextMeshProUGUI`(메시지), 하단 `Button` + 자식 `TextMeshProUGUI`(라벨)
3. 루트에 `UI_DemoResult` 컴포넌트 추가, `messageText` / `buttonLabel` 할당
4. Button의 `OnClick`에 루트를 할당하고 `UI_DemoResult.OnClickReturnToTitle` 선택

- [ ] **Step 7: `InitUI`에 등록**

`Branch/Assets/_GameAssets/04. Scenes/UI_Global.unity`를 열고 `InitUI`의 `uiInfos` 배열 Size를 1 늘려 추가한다.

- `Ui Type`: `DemoResult`
- `Ui Prefab`: `UI_DemoResult.prefab`
- `Is Active`: **체크 해제**

- [ ] **Step 8: `AmonEndPhase()`의 임시 호출 교체**

`Branch/Assets/_GameAssets/01. Scripts/Managers/DungeonManager.cs`

Task 4 Step 7에서 넣은 `TODO(Task 6)` 블록을 교체한다.

```csharp
            // 체험 플레이에서는 리스폰 대신 결과 화면으로 진입한다.
            if (GameManager.Instance.PlayMode == EPlayMode.Demo)
            {
                GameManager.Instance.EnterDemoResult();
                return;
            }
```

- [ ] **Step 9: 임시 디버그 컴포넌트 제거**

1. `Tutorial.unity`에서 `DebugReturn` 오브젝트 삭제
2. `Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs`와 `.meta` 삭제

```bash
git rm "Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs" "Branch/Assets/_GameAssets/01. Scripts/Test/DemoDebugReturn.cs.meta"
```

- [ ] **Step 10: 검증 — 결과 화면 표시**

`Bootstrap.unity` Play → "체험 플레이" → 프롤로그 → 튜토리얼 씬 →
Inspector에서 `AmonPaseTwoFSM.isEnabled` 체크 → 아몬 처치.

Console 기대 출력:
```
Amon Pase Two has died.
(5초 후)
[GameManager] 체험 플레이 결과 화면 진입
```

결과 패널이 표시되고, 마우스 커서가 보이며, 뒤에서 플레이어/몬스터가 정지해 있어야 한다.

- [ ] **Step 11: 검증 — 3회 왕복 (정식 경로)**

"타이틀로" 클릭 → 타이틀 복귀 → 다시 "체험 플레이".
**3회 반복**하며 매회 정상 로드되는지 확인한다.

Task 3 Step 4의 기대 로그가 매회 출력되어야 한다.

- [ ] **Step 12: 커밋**

```bash
git add -A "Branch/Assets/_GameAssets"
git commit -m "feat: 체험 플레이 결과 화면 추가 및 임시 디버그 복귀 제거"
```

---

## Task 7: TutorialDirector + 스텝 조건

튜토리얼 스텝 진행과 보스 기동을 씬 내부 컴포넌트로 통합한다.

**Files:**
- Create: `Branch/Assets/_GameAssets/01. Scripts/Tutorial/ITutorialCondition.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Tutorial/TutorialDirector.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/MoveCondition.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/AttackCondition.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/AreaEnterCondition.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/KillCountCondition.cs`
- Create: `Branch/Assets/_GameAssets/01. Scripts/GUI/UI_TutorialBanner.cs`
- Delete: `Branch/Assets/_GameAssets/01. Scripts/Test/TutorialProfileApplier.cs`
- Modify: `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity`

**Interfaces:**
- Consumes: `TutorialModeContext.Apply(TutorialBossProfile)` (Task 5), `Managers.DungeonManager.AmonSecondPhasePrefab` / `.PlayerTeleportPoint` / `.PlayerRespawnPoint`, `TutorialDataSO` (기존)
- Produces:
  - `ITutorialCondition.Satisfied` → `event Action`
  - `ITutorialCondition.Begin()` / `.End()` → `void`
  - `UI_TutorialBanner.Show(string title, string description)` / `.Hide()` → `void`

- [ ] **Step 1: `ITutorialCondition.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Tutorial/ITutorialCondition.cs`

```csharp
using System;

/// <summary>
/// 튜토리얼 스텝의 완료 조건. TutorialDirector가 스텝 진입 시 Begin(), 완료 시 End()를 호출한다.
/// 조건이 충족되면 Satisfied 이벤트를 한 번만 발생시킨다.
/// </summary>
public interface ITutorialCondition
{
    event Action Satisfied;
    void Begin();
    void End();
}
```

- [ ] **Step 2: `UI_TutorialBanner.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/GUI/UI_TutorialBanner.cs`

```csharp
using TMPro;
using UnityEngine;

/// <summary>
/// 인게임 튜토리얼 안내 배너. 화면 상단에 현재 스텝의 제목과 설명을 표시한다.
/// 기존 UI_Tutorial은 메뉴에서 여는 페이지 넘김식 도움말 팝업이라 성격이 달라 별도로 둔다.
/// </summary>
public class UI_TutorialBanner : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private void Awake() => Hide();

    public void Show(string title, string description)
    {
        if (root != null) root.SetActive(true);
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}
```

- [ ] **Step 3: `MoveCondition.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/MoveCondition.cs`

```csharp
using System;
using Managers;
using UnityEngine;

/// <summary>
/// 플레이어가 일정 거리 이상 이동하면 완료된다.
/// </summary>
public class MoveCondition : MonoBehaviour, ITutorialCondition
{
    [Tooltip("완료에 필요한 누적 이동 거리 (m)")]
    [SerializeField] private float requiredDistance = 5f;

    public event Action Satisfied;

    private bool _active;
    private bool _fired;
    private float _accumulated;
    private Vector3 _lastPosition;

    public void Begin()
    {
        _active = true;
        _fired = false;
        _accumulated = 0f;

        if (GameManager.Instance.Player != null)
            _lastPosition = GameManager.Instance.Player.transform.position;
    }

    public void End() => _active = false;

    private void Update()
    {
        if (!_active || _fired) return;
        if (GameManager.Instance.Player == null) return;

        Vector3 current = GameManager.Instance.Player.transform.position;
        _accumulated += Vector3.Distance(current, _lastPosition);
        _lastPosition = current;

        if (_accumulated < requiredDistance) return;

        _fired = true;
        Satisfied?.Invoke();
    }
}
```

- [ ] **Step 4: `AttackCondition.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/AttackCondition.cs`

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 공격 입력(마우스 좌클릭)이 지정 횟수만큼 발생하면 완료된다.
/// </summary>
public class AttackCondition : MonoBehaviour, ITutorialCondition
{
    [Tooltip("완료에 필요한 공격 입력 횟수")]
    [SerializeField] private int requiredCount = 3;

    public event Action Satisfied;

    private bool _active;
    private bool _fired;
    private int _count;

    public void Begin()
    {
        _active = true;
        _fired = false;
        _count = 0;
    }

    public void End() => _active = false;

    private void Update()
    {
        if (!_active || _fired) return;
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        _count++;
        if (_count < requiredCount) return;

        _fired = true;
        Satisfied?.Invoke();
    }
}
```

- [ ] **Step 5: `AreaEnterCondition.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/AreaEnterCondition.cs`

```csharp
using System;
using UnityEngine;

/// <summary>
/// 플레이어가 이 오브젝트의 트리거 볼륨에 진입하면 완료된다.
/// Collider를 Is Trigger로 설정해 함께 배치한다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AreaEnterCondition : MonoBehaviour, ITutorialCondition
{
    public event Action Satisfied;

    private bool _active;
    private bool _fired;

    public void Begin()
    {
        _active = true;
        _fired = false;
    }

    public void End() => _active = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_active || _fired) return;
        if (!other.CompareTag("Player")) return;

        _fired = true;
        Satisfied?.Invoke();
    }
}
```

- [ ] **Step 6: `KillCountCondition.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Tutorial/Conditions/KillCountCondition.cs`

```csharp
using System;
using System.Collections.Generic;
using Monster.AI.FSM;
using UnityEngine;

/// <summary>
/// 지정한 더미 몬스터가 전부 처치되면 완료된다.
/// FSM의 blackboard.CurrentHealth로 판정한다.
/// </summary>
public class KillCountCondition : MonoBehaviour, ITutorialCondition
{
    [Tooltip("처치해야 할 더미 몬스터의 FSM 목록")]
    [SerializeField] private List<FSM> targets = new();

    public event Action Satisfied;

    private bool _active;
    private bool _fired;

    public void Begin()
    {
        _active = true;
        _fired = false;
    }

    public void End() => _active = false;

    private void Update()
    {
        if (!_active || _fired) return;

        foreach (FSM target in targets)
        {
            if (target == null) continue;
            if (target.blackboard == null) continue;
            if (target.blackboard.CurrentHealth > 0f) return;
        }

        _fired = true;
        Satisfied?.Invoke();
    }
}
```

- [ ] **Step 7: `TutorialDirector.cs` 생성**

`Branch/Assets/_GameAssets/01. Scripts/Tutorial/TutorialDirector.cs`

```csharp
using System;
using System.Collections;
using Managers;
using Monster.AI.FSM;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct TutorialStep
{
    [Tooltip("표시할 안내 문구. 기존 TutorialDataSO를 재사용한다.")]
    public TutorialDataSO data;

    [Tooltip("ITutorialCondition을 구현한 컴포넌트")]
    public MonoBehaviour condition;

    public UnityEvent onEnter;
    public UnityEvent onComplete;
}

/// <summary>
/// 체험 플레이의 5분 시퀀스를 담당한다.
/// 매니저는 "어느 씬 세트를 쓰는가"만 알고, 시퀀스의 내용은 전부 이 컴포넌트에 격리한다.
/// </summary>
public class TutorialDirector : MonoBehaviour
{
    [Header("스텝")]
    [SerializeField] private TutorialStep[] steps;
    [SerializeField] private UI_TutorialBanner banner;

    [Header("보스")]
    [SerializeField] private FSM amonPhaseTwo;
    [SerializeField] private Transform playerTeleportPoint;
    [SerializeField] private Transform playerRespawnPoint;

    [Header("밸런싱")]
    [SerializeField] private TutorialBossProfile bossProfile;

    [Tooltip("보스전 시작 후 이 시간이 지나면 보스 체력을 서서히 깎는다. 0이면 비활성.")]
    [SerializeField] private float softTimeoutSeconds = 0f;

    [Tooltip("소프트 타임아웃 시 초당 깎을 최대 체력 비율 (0.01 = 1%)")]
    [SerializeField] private float softTimeoutDrainPerSecond = 0.02f;

    private int _currentIndex = -1;
    private ITutorialCondition _currentCondition;

    private void Awake()
    {
        // 배수 주입은 반드시 Awake에서 한다.
        // 같은 씬의 아몬 Awake/OnEnable이 먼저 실행될 수 있지만,
        // FSM.Start()가 Init()을 무조건 다시 호출하고 Start는 모든 Awake 이후에 실행되므로
        // 최종 스탯에는 배수가 반영된다. (FSM.Start()의 무조건 Init() 호출을 조건부로 바꾸지 말 것)
        TutorialModeContext.Apply(bossProfile);
    }

    private void Start()
    {
        AdvanceStep();
    }

    private void AdvanceStep()
    {
        // 이전 스텝 정리
        if (_currentCondition != null)
        {
            _currentCondition.Satisfied -= OnConditionSatisfied;
            _currentCondition.End();
            _currentCondition = null;
        }

        if (_currentIndex >= 0 && _currentIndex < steps.Length)
        {
            steps[_currentIndex].onComplete?.Invoke();
        }

        _currentIndex++;

        if (_currentIndex >= steps.Length)
        {
            StartBossFight();
            return;
        }

        TutorialStep step = steps[_currentIndex];

        // 안내 문구 표시
        if (banner != null && step.data != null)
        {
            bool isKorean = LocalizationManager.IsKorean;
            string title = isKorean ? step.data.title : step.data.enTitle;
            string[] descriptions = isKorean ? step.data.descriptions : step.data.enDescriptions;
            string description = descriptions is { Length: > 0 } ? descriptions[0] : string.Empty;

            banner.Show(title, description);
        }

        step.onEnter?.Invoke();

        // 조건 구독
        _currentCondition = step.condition as ITutorialCondition;
        if (_currentCondition == null)
        {
            Debug.LogWarning($"[TutorialDirector] 스텝 {_currentIndex}의 condition이 ITutorialCondition이 아닙니다. 즉시 다음 스텝으로 진행합니다.");
            AdvanceStep();
            return;
        }

        _currentCondition.Satisfied += OnConditionSatisfied;
        _currentCondition.Begin();

        Debug.Log($"[TutorialDirector] 스텝 {_currentIndex} 시작: {step.data?.key}");
    }

    private void OnConditionSatisfied() => AdvanceStep();

    private void StartBossFight()
    {
        Debug.Log("[TutorialDirector] 튜토리얼 완료, 보스전 시작");

        if (banner != null) banner.Hide();

        if (amonPhaseTwo == null)
        {
            Debug.LogError("[TutorialDirector] amonPhaseTwo가 할당되지 않아 보스전을 시작할 수 없습니다.");
            return;
        }

        // AmonPaseTwoFSM 사망 시 DungeonManager.AmonEndPhase()가 호출되므로 참조를 주입해둔다.
        DungeonManager.Instance.AmonSecondPhasePrefab = amonPhaseTwo;
        DungeonManager.Instance.PlayerTeleportPoint = playerTeleportPoint;
        DungeonManager.Instance.PlayerRespawnPoint = playerRespawnPoint;

        amonPhaseTwo.isEnabled = true;

        if (softTimeoutSeconds > 0f)
        {
            StartCoroutine(SoftTimeoutRoutine());
        }
    }

    // 보스전이 길어지면 체력을 서서히 깎아 5분 예산 상한을 지킨다.
    private IEnumerator SoftTimeoutRoutine()
    {
        yield return new WaitForSeconds(softTimeoutSeconds);

        Debug.Log("[TutorialDirector] 소프트 타임아웃 시작");

        while (amonPhaseTwo != null && amonPhaseTwo.blackboard != null &&
               amonPhaseTwo.blackboard.CurrentHealth > 0f)
        {
            float drain = amonPhaseTwo.blackboard.MaxHealth * softTimeoutDrainPerSecond * Time.deltaTime;
            amonPhaseTwo.blackboard.CurrentHealth -= drain;
            yield return null;
        }
    }
}
```

- [ ] **Step 8: 튜토리얼 배너 UI 배치**

`Tutorial.unity`에 씬 전용 Canvas를 만든다. (`Scene_UI`는 본편과 공유되므로 튜토리얼 전용 UI는 씬에 둔다.)

1. `GameObject > UI > Canvas` 생성, 이름 `TutorialCanvas`, Render Mode `Screen Space - Overlay`
2. 자식으로 `Banner` 패널 생성 (화면 상단, 반투명 배경)
3. `Banner` 자식으로 `Title` / `Description` TextMeshProUGUI 생성
4. `TutorialCanvas`에 `UI_TutorialBanner` 컴포넌트 추가 → `root`에 `Banner`, `titleText` / `descriptionText` 할당

- [ ] **Step 9: 튜토리얼 콘텐츠 SO 작성**

`Branch/Assets/Resources/Tutorial/` 에서 `Create > Scriptable Object > Tutorial Data`로 3개 생성한다.

| 파일명 | key | title / enTitle | descriptions[0] / enDescriptions[0] |
|---|---|---|---|
| `Demo_01_Move` | `demo_move` | `이동` / `Movement` | `WASD로 이동하세요.` / `Use WASD to move.` |
| `Demo_02_Attack` | `demo_attack` | `공격` / `Attack` | `마우스 좌클릭으로 공격하세요.` / `Left-click to attack.` |
| `Demo_03_Arena` | `demo_arena` | `아레나로` / `To the Arena` | `앞쪽 문으로 이동하세요.` / `Head to the door ahead.` |

- [ ] **Step 10: 씬에 Director와 조건 배치**

`Tutorial.unity`에서:

1. 빈 오브젝트 `TutorialDirector` 생성 → `TutorialDirector` 컴포넌트 추가
2. 자식 `Cond_Move` 생성 → `MoveCondition` 추가 (`requiredDistance = 5`)
3. 자식 `Cond_Attack` 생성 → `AttackCondition` 추가 (`requiredCount = 3`)
4. 자식 `Cond_Arena` 생성 → `BoxCollider`(Is Trigger 체크) + `AreaEnterCondition` 추가, Position `(0, 1, 10)`, Size `(6, 3, 1)`
5. `TutorialDirector`의 `steps` Size를 `3`으로 설정하고 각 원소를 위 SO와 조건 컴포넌트로 연결
6. `banner`에 `TutorialCanvas` 할당
7. `amonPhaseTwo` / `playerTeleportPoint` / `playerRespawnPoint` / `bossProfile` 할당
8. `softTimeoutSeconds`는 `0`으로 둔다 (Task 9에서 결정)

- [ ] **Step 11: 임시 컴포넌트 제거**

1. `Tutorial.unity`에서 `BossActivator` 오브젝트 삭제 (`SceneTestFSMActivator` + `TutorialProfileApplier`)
2. `TutorialProfileApplier.cs` 삭제

```bash
git rm "Branch/Assets/_GameAssets/01. Scripts/Test/TutorialProfileApplier.cs" "Branch/Assets/_GameAssets/01. Scripts/Test/TutorialProfileApplier.cs.meta"
```

> `SceneTestFSMActivator.cs` 자체는 다른 씬 테스트에 쓰이므로 **삭제하지 않는다.**

- [ ] **Step 12: 검증 — 전체 플로우**

`Bootstrap.unity` Play → "체험 플레이" → 프롤로그 → 튜토리얼 씬.

Console 기대 출력 (순서대로):
```
[TutorialModeContext] 배수 적용 - HP x0.4, DMG x0.6
[TutorialDirector] 스텝 0 시작: demo_move
```

배너에 "이동 / WASD로 이동하세요."가 표시되어야 한다.

이동 → `[TutorialDirector] 스텝 1 시작: demo_attack`
공격 3회 → `[TutorialDirector] 스텝 2 시작: demo_arena`
트리거 진입 →
```
[TutorialDirector] 튜토리얼 완료, 보스전 시작
Amon Pase Two has spawned.
```

**순서 검증**: 아몬 기동 후 `AmonPaseTwoFSM.Think()`가 출력하는 `{CurrentHealth} / {MaxHealth}`
로그에서 `MaxHealth`가 시트 원본의 `healthMultiplier`배인지 확인한다.
원본 값 그대로라면 `TutorialModeContext` 주입이 `FSM.Start()`보다 늦은 것이므로
`TutorialDirector.Awake()`의 주입 위치를 다시 확인한다.

보스 처치 → `[GameManager] 체험 플레이 결과 화면 진입`

- [ ] **Step 13: 검증 — 3회 왕복**

"타이틀로" → "체험 플레이"를 **3회 반복**하며 매회 스텝 0부터 정상 시작되는지 확인한다.

`_currentIndex`는 씬과 함께 파괴되고 재생성되므로 자동으로 초기화된다.
2회차에 `스텝 0 시작` 로그가 나오지 않으면 씬 언로드가 실패한 것이다.

- [ ] **Step 14: 커밋**

```bash
git add -A "Branch/Assets/_GameAssets" Branch/Assets/Resources
git commit -m "feat: TutorialDirector와 스텝 조건 컴포넌트 추가"
```

---

## Task 8: 밸런싱 (5분 예산 실측)

**Files:**
- Modify: `Branch/Assets/Resources/Tutorial/TutorialBossProfile.asset`
- Modify: `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity` (조건 파라미터 조정)

**Interfaces:**
- Consumes: Task 5·7의 전체 플로우
- Produces: 확정된 `healthMultiplier` / `damageMultiplier` 값

- [ ] **Step 1: 프롤로그 실측**

`Bootstrap.unity` Play → "체험 플레이" → 스톱워치로 프롤로그 시작부터 튜토리얼 씬 진입까지 측정.

3회 측정해 평균을 기록한다. 스펙 §6의 예산표를 실측값으로 갱신한다.

- [ ] **Step 2: 튜토리얼 구간 실측**

튜토리얼 씬 진입부터 `[TutorialDirector] 튜토리얼 완료` 로그까지 측정.

목표 70~80초를 넘으면 `MoveCondition.requiredDistance` / `AttackCondition.requiredCount`를 낮춘다.

- [ ] **Step 3: 보스전 실측 및 배수 역산**

`healthMultiplier`를 `0.4`로 두고 보스전 시간을 3회 측정한다.

목표 시간 계산:
```
목표 보스전 시간 = 300 - (프롤로그 실측) - (튜토리얼 실측) - 5(등장) - 5(사망) - 20(결과)
```

실측이 목표보다 길면 `healthMultiplier`를 비례해서 낮춘다.
예: 목표 135초, 실측 180초 → `0.4 × (135 / 180) = 0.3`

- [ ] **Step 4: 난이도 확인**

`damageMultiplier`를 조정해 "숙련자가 아니어도 2~3회 사망 안에 클리어" 수준을 맞춘다.
사망이 4회를 넘으면 `damageMultiplier`를 낮춘다.

- [ ] **Step 5: 전체 플로우 3회 측정**

타이틀 클릭부터 결과 화면 "타이틀로" 클릭까지 **3회** 측정해 전부 300초 이내인지 확인한다.

- [ ] **Step 6: 스펙 예산표 갱신 및 커밋**

`docs/superpowers/specs/2026-08-25-tutorial-demo-flow-design.md` §6의 표를 실측값으로 갱신한다.

```bash
git add -A "Branch/Assets" docs
git commit -m "chore: 체험 플레이 밸런싱 실측값 반영"
```

---

## Task 9 (선택): 소프트 타임아웃 활성화

> **Task 8에서 보스전 편차가 크지 않다면 이 태스크는 건너뛴다.**

`TutorialDirector.SoftTimeoutRoutine()`은 Task 7에서 이미 구현되어 있고 `softTimeoutSeconds = 0`으로 비활성 상태다. 이 태스크는 값 설정과 검증만 수행한다.

**Files:**
- Modify: `Branch/Assets/_GameAssets/04. Scenes/GameScene/Tutorial.unity`

**Interfaces:**
- Consumes: `TutorialDirector.softTimeoutSeconds` / `.softTimeoutDrainPerSecond` (Task 7)
- Produces: 없음

- [ ] **Step 1: 값 설정**

`Tutorial.unity`의 `TutorialDirector`에서:

- `softTimeoutSeconds`: Task 8에서 측정한 목표 보스전 시간 × 1.3
- `softTimeoutDrainPerSecond`: `0.02` (초당 최대 체력 2%)

- [ ] **Step 2: 검증 — 발동**

Play 후 보스전에서 **일부러 공격하지 않고** 대기한다.

Console 기대 출력 (설정 시간 경과 후):
```
[TutorialDirector] 소프트 타임아웃 시작
```

이후 `AmonPaseTwoFSM.Think()`가 출력하는 `{CurrentHealth} / {MaxHealth}` 로그에서
`CurrentHealth`가 공격 없이도 감소해야 한다.

- [ ] **Step 3: 검증 — 미발동**

정상 속도로 플레이해 타임아웃 전에 클리어되는지 확인한다.
`[TutorialDirector] 소프트 타임아웃 시작` 로그가 **출력되지 않아야** 한다.

- [ ] **Step 4: 커밋**

```bash
git add "Branch/Assets/_GameAssets/04. Scenes"
git commit -m "feat: 체험 플레이 소프트 타임아웃 활성화"
```

---

## Task 0 결과

> Task 0 수행 후 여기에 판정을 기록한다.

- 확인 일시:
- `Player` / `Minimap` / `FollowCamera`의 소속 씬:
- 판정: 결함 C 재현 여부 (재현 / 미재현)
- Task 2 수행 여부:
