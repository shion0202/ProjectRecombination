# 튜토리얼 + 저스펙 보스전 체험 플로우 설계

- 작성일: 2026-08-25
- 목적: 게임 행사 출품용 5분 분량 체험 플레이 추가
- 개발 가용 기간: 1~2주

---

## 1. 배경과 목표

본편은 공략을 전부 숙지한 상태에서도 10~20분이 소요되어 행사 부스 운영에 부적합하다.
5분 안에 게임의 핵심 경험(전투 + 보스전)을 전달하는 별도 체험 플로우를 추가한다.

일반적인 튜토리얼처럼 기본 조작만 알려주고 끝나면 게임의 핵심인 보스가 빠져 체험의 의도가
퇴색되므로, **짧은 튜토리얼 직후 저스펙 보스전**을 배치한다.

### 결정된 방향

| 항목 | 결정 |
|---|---|
| 진입 | 타이틀에 **별도 버튼** 추가 (본편 플로우와 병렬) |
| 순서 | 타이틀 → **프롤로그(기존 재사용)** → 튜토리얼 씬 → 보스전 → 결과 → 타이틀 |
| 보스 | **아몬 2페이즈 단독** (1페이즈는 전투 밀도가 낮아 시연에 부적합) |
| 보스 스펙 | **런타임 배수 오버라이드** (구글 시트 미수정) |
| 사망 | 무제한 부활 (본편과 동일) |
| 종료 | 전용 결과 화면 → 타이틀 복귀 |
| 반복 플레이 | **게임을 끄지 않고 종일 반복 실행**되므로 세션 리셋이 필수 요구사항 |

### 이번 범위에서 제외

- 맵 디자인 (별도 진행 — §8의 규약만 제공)
- 중간보스(익스큐셔너) 분기 선택 — 여유 시 추후 고려
- 프롤로그 스킵 기능 — 같은 사람이 반복 플레이하는 경우가 드물어 우선순위 낮음
- 본편 `ExitGame` / 크레딧 경로의 동일 계열 버그 수정 (§7 참조)

---

## 2. 현재 구조 파악

### 부트 체인

```
Bootstrap.unity
  └ InitBootstrap.Start()
      └ Addressables → Scene_Persistent (SetActiveScene)
          └ InitPersistent.Awake() : 매니저 프리팹 Instantiate
              └ OnDestroy() → GameManager.EnterTitle()
                  └ GUIManager.LoadGUI() → Scene_UI
                      └ InitUI.Awake() → GUIManager.Init(uiInstances)
```

### 플레이 진입

```
UI_Title.OnClickStart()
  └ GameManager.EnterPrologue()
      ├ State = Loading
      ├ DungeonManager.Init()      : stageDatas[0], [1] 로드 + 미니맵 Instantiate
      ├ PoolManager.Init()
      ├ LoadPlayerScene()          : Scene_Player
      ├ DungeonManager.SetPlayerStartPosition()
      └ State = Prologue
          └ UI_Prologue : 영상 4컷 + 타이핑 자막 (로딩 포함 40~60초)
              └ FadePanelAndGoToNext() → GameManager.StartGame()
                  └ State = Playing + Player.PlayIntroSequence(4.0f)
```

### 스테이지 로드/언로드

`DungeonManager.stageDatas[]`가 `stageIndex ↔ stageName(Addressable 주소)` 매핑을 갖고,
현재 인덱스와 인접 인덱스만 로드된 상태를 유지한다.
`StageUpdateTrigger.OnTriggerExit`가 `UpdatePlayerStageIndex(i + 1)`을 호출해 진행한다.

### 보스 기동 경로

```
BossTrigger.OnTriggerEnter (플레이어 진입)
  └ DungeonManager에 FSM/좌표 참조 주입
      · AmonFirstPhasePrefab, AmonSecondPhasePrefab
      · PlayerTeleportPoint, PlayerRespawnPoint

StartAmonFirstPhase (Visual Scripting)
  └ DungeonManager.AmonFirstPhase()  → amonFirstPhase.isEnabled = true
      └ AmonPaseOneFSM 사망 → DungeonManager.AmonSecondPhase()
          · 1페이즈 FSM off
          · CharacterController 껐다 켜며 플레이어를 playerTeleportPoint로 이동
          · amonSecondPhasePrefab.isEnabled = true
              └ AmonPaseTwoFSM 사망 → 5초 후 DungeonManager.AmonEndPhase()
                  · 2페이즈 FSM off
                  · 플레이어를 playerRespawnPoint로 이동
```

### 몬스터 스탯 파이프라인

```
FSM.Init() → Blackboard.Init() → InitMonsterStatsByID()
  └ id(1000~1999) → DataManager.GetRowDataByIndex("CharacterStats", id)
      └ _map["maxHealth"], ["damage"], ["walkSpeed"], ["defence"] ... 채움
  └ CurrentHealth = MaxHealth
  └ Target = MonsterManager.Instance.Player
```

`Blackboard.Init()`은 `FSM.Start()` / `FSM.OnEnable()` / `FSM.Update()`(`!isInit`일 때)에서
호출되므로 **여러 번 실행될 수 있다.** 매번 시트에서 새로 읽어오므로 값이 누적되지는 않는다.

### 기존 자산 — 재사용 가능한 것

| 자산 | 위치 | 활용 |
|---|---|---|
| `SceneTestLauncher.LoadInGame()` | `01. Scripts/Editor/` | "임의 스테이지를 플레이 가능 상태로 띄우는" 순서의 검증된 레퍼런스 |
| `SceneTestFSMActivator` | `01. Scripts/Test/` | **아몬 2페이즈를 1페이즈 없이 단독 기동하는 경로가 이미 검증됨** |
| `TutorialDataSO` | `01. Scripts/Data/Tutorial/` | 한/영 튜토리얼 텍스트 구조 (`Resources/Tutorial`에서 로드) |
| `TestManager` | `01. Scripts/Test/` | 정적 홀더 패턴의 선례 (`TutorialModeContext`가 이를 따름) |
| `GameManager.PauseObjects/UnpauseObjects` | `01. Scripts/Managers/` | 튜토리얼 연출 중 입력 차단에 활용 가능 |

**핵심**: 아몬 2페이즈 단독 소환은 신규 개발이 아니라 이미 팀이 테스트해온 경로다.
남은 의존성은 §5에 정리한 4개뿐이다.

---

## 3. 아키텍처 방침

> 매니저는 "어느 씬 세트를 쓰는가"만 알고, **5분 시퀀스의 내용은 전부 튜토리얼 씬 안에 격리**한다.

본편 코드 수정을 최소화하고, 행사 이후 유지보수 부담을 낮추기 위한 원칙이다.
본편에 들어가는 변경은 다음 4가지로 한정한다.

1. `GameManager` — 모드 플래그 + 결과 화면 상태
2. `DungeonManager` — 스테이지 세트 분기 + 세션 리셋
3. `Blackboard` — 스탯 배수 적용 지점 3줄
4. §7 세션 리셋 (반복 플레이 안정성 — 본편에도 이득)

---

## 4. 컴포넌트 설계

### 4.1 모드 플래그 (`GameManager`)

```csharp
public enum EPlayMode { Normal, Demo }

public EPlayMode PlayMode { get; private set; } = EPlayMode.Normal;

public async void EnterPrologue(EPlayMode mode)   // 기존 시그니처에 인자 추가
{
    PlayMode = mode;
    if (mode == EPlayMode.Demo) IsHardMode = false;   // ※ 아래 주석 참조
    // ... 기존 로직
}
```

`TutorialModeContext`(§4.4)의 활성화는 여기서 하지 않는다. 배수 값과 함께
`TutorialDirector.Awake()`가 단일 소유자로 주입한다(§4.4 말미의 순서 근거 참조).

`UI_Title`에 `OnClickDemoStart()` 추가 → `EnterPrologue(EPlayMode.Demo)`.
기존 `OnClickStart()`는 `EnterPrologue(EPlayMode.Normal)`.

**프롤로그는 양쪽이 공유한다.** (40~60초로 5분 예산 내 수용 가능 — §6)

> **`IsHardMode = false` 강제 이유**
> 하드 모드에서는 사망 시 `RebirthGame()`이 `DungeonManager.ResetCurrentStage()`를 호출해
> 현재 스테이지 씬을 통째로 리로드한다. 튜토리얼 씬은 스테이지가 1개뿐이라 이것이 곧
> **보스전 전체 초기화**를 의미하므로 데모에서는 반드시 꺼야 한다.

### 4.2 스테이지 세트 분기 (`DungeonManager`)

```csharp
[SerializeField] private StageData[] stageDatas;
[SerializeField] private StageData[] demoStageDatas;   // 원소 1개: 튜토리얼 씬

private StageData[] CurrentStageSet =>
    GameManager.Instance.PlayMode == EPlayMode.Demo ? demoStageDatas : stageDatas;
```

`Init()` / `UnloadAllStage()` / `ResetCurrentStage()` / `UpdatePlayerStageIndex()`의
`stageDatas` 참조를 전부 `CurrentStageSet`으로 교체한다.
스테이지가 1개뿐이라 `StageUpdateTrigger`는 데모에서 동작하지 않는다.

`Init()`의 인접 스테이지 로드 루프(`stageIndex == 0 || stageIndex == 1`)는 원소가 1개면
자연스럽게 그 1개만 로드하므로 로직 수정이 불필요하다.

**튜토리얼 씬은 Addressable 등록이 필수다.** `SceneController`가 `Addressables.LoadSceneAsync`를
사용하므로 주소(`Scene_Tutorial`)가 없으면 로드되지 않는다.

### 4.3 튜토리얼 시퀀스 (튜토리얼 씬 내부)

```csharp
public class TutorialDirector : MonoBehaviour
{
    [SerializeField] private TutorialStep[] steps;
    [SerializeField] private FSM amonPhaseTwo;          // 씬에 isEnabled=false로 배치
    [SerializeField] private Transform arenaAnchor;
    [SerializeField] private Transform playerTeleportPoint;
    [SerializeField] private Transform playerRespawnPoint;
    [SerializeField] private float softTimeoutSeconds = 0f;   // 0이면 비활성 (선택 항목)
}

[Serializable]
public struct TutorialStep
{
    public TutorialDataSO data;        // 기존 SO 재사용 (한/영 텍스트 보유)
    public MonoBehaviour condition;    // ITutorialCondition 구현체
    public UnityEvent onEnter;
    public UnityEvent onComplete;
}

public interface ITutorialCondition
{
    event Action Satisfied;
    void Begin();
    void End();
}
```

구현할 조건 컴포넌트 (4개면 충분):

| 컴포넌트 | 완료 조건 |
|---|---|
| `MoveCondition` | 이동 입력이 일정 시간/거리 이상 발생 |
| `AttackCondition` | 공격 입력 N회 |
| `AreaEnterCondition` | 지정 트리거 볼륨 진입 |
| `KillCountCondition` | 더미 몬스터 N마리 처치 |

**`TutorialDataSO` 재사용이 핵심이다.** 이미 `key / title / descriptions[] / enTitle /
enDescriptions[] / exampleImage` 구조가 있고 `Resources/Tutorial`에서 로드되므로,
기획 쪽이 스크립트 수정 없이 바로 문구를 작성할 수 있고 기존 도움말 패널(`UI_Tutorial`)과
문구를 공유할 수 있다.

표시는 **새 배너 UI**로 만든다. 기존 `UI_Tutorial`은 페이지 넘김식 팝업(메뉴에서 여는 도움말)이라
인게임 진행형 안내와 성격이 달라 재사용하지 않는다. 단 `LocalizationManager.IsKorean` 분기
패턴은 동일하게 따른다.

**마지막 스텝 완료 시:**

```csharp
DungeonManager.Instance.AmonSecondPhasePrefab = amonPhaseTwo;
DungeonManager.Instance.PlayerTeleportPoint   = playerTeleportPoint;
DungeonManager.Instance.PlayerRespawnPoint    = playerRespawnPoint;
amonPhaseTwo.isEnabled = true;
```

`SceneTestFSMActivator.OnTestStart()`가 하던 것과 동일한 동작이다.
`DungeonManager`에 참조를 주입하는 이유는 `AmonPaseTwoFSM`이 사망 시
`DungeonManager.AmonEndPhase()`를 호출하는데, 참조가 비어 있으면 NRE가 발생하기 때문이다.

### 4.4 보스 저스펙 (런타임 배수)

```csharp
/// TestManager와 동일한 정적 홀더 패턴. 데모 모드에서만 값이 설정된다.
public static class TutorialModeContext
{
    public static bool  IsActive;
    public static float BossHealthMultiplier = 1f;
    public static float BossDamageMultiplier = 1f;

    public static void Reset()
    {
        IsActive = false;
        BossHealthMultiplier = 1f;
        BossDamageMultiplier = 1f;
    }
}
```

적용 지점은 `Blackboard.InitMonsterStatsByID()`, 시트에서 `_map`을 채운 **직후**:

```csharp
if (TutorialModeContext.IsActive)
{
    _map["maxHealth"] = (float)_map["maxHealth"] * TutorialModeContext.BossHealthMultiplier;
    _map["health"]    = _map["maxHealth"];
    _map["damage"]    = (float)_map["damage"]    * TutorialModeContext.BossDamageMultiplier;
}
```

**이 위치라야 안전한 이유**: `Init()`이 여러 번 호출되어도 매번 시트에서 원본 값을 새로
읽어오므로 배수가 중첩 적용되지 않는다. `Init()` 외부에서 스탯을 곱하는 방식은
재초기화 시 값이 계속 줄어드는 버그가 된다.

배수 값은 `TutorialBossProfile` ScriptableObject에 두고 인스펙터에서 조절한다.
`TutorialDirector`가 시작 시 프로필을 `TutorialModeContext`에 주입한다.

**플레이어 공격력을 올리는 대신 보스 HP를 낮추는 방향으로 통일한다.**
`TestManager.EnemyDamageMultiplier`(에디터 테스트 전용)와 밸런싱 출처가 섞이면
디버깅이 어려워진다.

> **알려진 부작용**: 이 배수는 데모 모드의 **모든 몬스터**에 적용된다.
> 튜토리얼 더미도 함께 약해지는데 일반적으로는 바람직하므로 그대로 둔다.
> 보스에만 적용하려면 `id` 범위로 게이트할 수 있다.

#### 적용 시점의 순서 근거

`TutorialModeContext`의 값은 `TutorialDirector.Awake()`가 주입한다. 이때
같은 씬에 있는 아몬 오브젝트의 `Awake` / `OnEnable`이 `TutorialDirector.Awake()`보다
**먼저 실행될 수 있다.** Unity는 동일 씬 내 컴포넌트의 `Awake` 순서를 보장하지 않는다.

그럼에도 안전한 이유는 `FSM.Start()`가 `Init()`을 **무조건 다시 호출**하기 때문이다.
`Start()`는 해당 씬의 모든 `Awake`가 끝난 뒤 실행되므로, `TutorialDirector.Awake()`의
주입은 반드시 `FSM.Start()` → `Blackboard.Init()` → `InitMonsterStatsByID()`보다 앞선다.
`InitMonsterStatsByID()`가 매번 시트에서 원본을 새로 읽으므로 최종 값은 항상 올바르다.

이 성질이 깨지지 않도록, `FSM.Start()`의 무조건 `Init()` 호출을 조건부로 바꾸지 않는다.

### 4.5 종료 및 결과 화면

`AmonPaseTwoFSM`은 사망 5초 후 `DungeonManager.AmonEndPhase()`를 호출한다. 여기에 모드 분기:

```csharp
public void AmonEndPhase()
{
    amonSecondPhasePrefab.isEnabled = false;

    if (GameManager.Instance.PlayMode == EPlayMode.Demo)
    {
        GameManager.Instance.EnterDemoResult();
        return;
    }

    // 기존 플레이어 리스폰 이동 로직
}
```

추가 항목:

- `GameManager.GameState.DemoResult` 추가
- `EUIType.DemoResult` 추가 (`EUIType.cs`)
- `GUIManager.Update()`의 상태 switch에 `case DemoResult` 추가
- `Scene_UI`의 `InitUI.uiInfos`에 결과 패널 프리팹 등록
- 결과 패널: 간단한 감사 문구 + "타이틀로" 버튼 (한/영)

"타이틀로" → §7의 `ReturnToTitleFromDemo()` → `EnterTitle()`

**사망 처리는 본편과 동일한 무제한 부활**(`RebirthGame()`, 5초 대기)을 그대로 사용한다.
`IsHardMode`가 false이므로 스테이지 리셋 없이 제자리 부활한다.

---

## 5. 아몬 2페이즈 단독 기동 — 의존성 체크리스트

| # | 의존성 | 실체 | 충족 방법 |
|---|---|---|---|
| 1 | `blackboard.Target` | `Blackboard.Init()`에서 `MonsterManager.Instance.Player` | **Scene_Player를 튜토리얼 씬보다 먼저 로드**. `InitPlayer.Init()`이 `MonsterManager.Player`를 설정하므로 순서가 어긋나면 보스가 플레이어를 인식하지 못한다 |
| 2 | `arenaAnchor` | 스킬의 워프/스폰 좌표 기준 Transform. 미할당 시 월드 좌표로 폴백 | 튜토리얼 맵 아레나 중심에 배치 후 `Blackboard.arenaAnchor`에 할당. **누락 시 스킬이 엉뚱한 위치에서 발동** |
| 3 | `AmonEndPhase()` | 사망 후 `playerRespawnPoint`로 이동. 원래 `BossTrigger`가 주입 | §4.3에서 `TutorialDirector`가 주입 + §4.5 모드 분기 |
| 4 | `UpdateBossHpBar` | `GUIManager.Instance.GameUIController` | `Scene_UI`가 이미 로드된 상태이므로 그대로 동작 |
| 5 | NavMesh | `AmonPaseTwoFSM.ActChase()`가 `NavMeshAgent.SetDestination` 사용 | 튜토리얼 맵에 NavMesh 베이크 필수. **없으면 보스가 추적하지 않는다** |

`AmonPaseTwoFSM.Init()`은 `ChangeState("Spawn")`으로 시작해 `spawnWaitTime`(기본 5초)의
등장 연출을 재생한 뒤 전투에 들어간다. 이 5초는 5분 예산에 포함해야 한다.

---

## 6. 5분 예산

| 구간 | 배정 | 근거 |
|---|---|---|
| 프롤로그 | 40~60초 | 영상 4컷, 실측 기준 로딩 포함 약 40초. 자막을 천천히 읽으면 60초 |
| 튜토리얼 | 70~80초 | 스텝 4~5개 |
| 보스 등장 연출 | 5초 | `AmonPaseTwoFSM.spawnWaitTime` |
| 보스전 | ~135초 | `BossHealthMultiplier`로 조절 |
| 사망 연출 | 5초 | `AmonPaseTwoFSM.deathWaitTime` |
| 결과 화면 | ~20초 | |
| **합계** | **275~305초** | 목표 300초 |

프롤로그가 상한(60초)을 치면 총합이 300초를 살짝 넘는다. 조정 여지는 보스전 배정뿐이므로
밸런싱 단계에서 프롤로그 실측치를 먼저 확정한 뒤 `BossHealthMultiplier`를 역산한다.

### 편차 리스크

실질 변수는 **보스전 하나**다. 튜토리얼은 스텝 조건 충족 방식이라 편차가 작지만,
보스전은 체험자 실력에 따라 90초~4분까지 벌어질 수 있다.

`BossHealthMultiplier` 하나로는 하한(숙련자가 40초에 종료)과 상한(초보가 못 깸)을
동시에 잡을 수 없다. 상한이 행사 운영에 더 치명적이므로 **선택 항목**으로 다음을 둔다.

> **소프트 타임아웃 (선택)**
> `TutorialDirector.softTimeoutSeconds` 경과 후부터 보스 HP를 초당 일정 비율로 감소시킨다.
> 체험자는 "내가 이겼다"고 느끼고, 운영은 회전율을 확보한다.
> `softTimeoutSeconds = 0`이면 비활성. 일정에 여유가 있을 때만 구현한다.

---

## 7. 반복 플레이 세션 리셋 (필수 요구사항)

행사에서는 게임을 끄지 않고 종일 반복 실행한다. 현재 코드에는 2회차 진입을 막는 결함이
다수 존재하므로 이를 해결하지 않으면 체험 플로우 자체가 성립하지 않는다.

### 7.1 확인된 결함

#### A. `DungeonManager._isInit`이 복구되지 않음 — **치명적**

`Init()`에서 `true`가 된 뒤 어디서도 `false`로 되돌아가지 않는다.
2회차 `EnterPrologue()`의 `Init()`이 즉시 return하여 **스테이지가 아예 로드되지 않는다.**
팀이 관찰한 "반복 플레이 시 스테이지 로드 문제"의 유력한 주범이다.

#### B. `DungeonManager.LoadedStages`가 비워지지 않음 — **치명적**

`UnloadAllStage()`는 `SceneController`에만 언로드를 지시하고 자신의 딕셔너리는 그대로 둔다.
A를 고쳐도 `LoadStage()`가 "이미 로드됨"으로 판단해 스킵한다.

#### C. 런타임 생성 오브젝트가 Persistent 씬에 누적됨 — **치명적**

> **전제 — Active Scene 고정은 의도된 설계다.**
> `InitBootstrap`이 `Scene_Persistent`를 Active Scene으로 설정하고,
> `SceneController`의 `SetActiveScene()` 호출이 전부 주석 처리된 것
> (`DungeonManager.cs:85`, `:148`, `:318`)은 **버그가 아니다.**
> 멀티 씬 환경에서 Active Scene이 바뀌면 베이크된 라이팅 결과와 컬링이
> 제대로 적용되지 않는 문제가 있어 의도적으로 고정한 것이다.
> **이 동작은 변경 대상이 아니며, 아래 해결책은 이 제약을 전제로 설계한다.**

Active Scene이 Persistent로 고정되어 있으므로, 부모를 지정하지 않은 `Instantiate`의
결과물은 전부 `Scene_Persistent`에 생성된다.

`InitPlayer.Awake()`는 Player / Minimap / FollowCamera / Volume 등을 부모 지정 없이
`Instantiate`하므로 이들은 **`Scene_Player`가 아니라 `Scene_Persistent`에 생성된다.**
`Player.unity`는 GameObject 5개 규모의 로더 씬이며 실제 오브젝트를 담고 있지 않다.

결과적으로 `UnloadPlayerScene()`은 빈 로더 씬만 언로드하고,
**Player·카메라·미니맵은 살아남아 2회차에 중복 생성된다.**
`GameManager.Player` 참조는 새 인스턴스로 덮이지만 구 인스턴스는 계속 `Update`를 돈다.

`DungeonManager.LoadMiniMap()`의 `Instantiate(miniMapPrefab)`도 동일하게 매 판 누적된다.

> **검증 방법**: 데모를 2회차 진입시킨 뒤
> `FindObjectsOfType<PlayerController>().Length`를 확인하거나
> Hierarchy에서 `Scene_Persistent` 하위를 관찰한다.
> 이 항목은 코드 정적 분석에 근거한 추정이므로 **구현 착수 전 런타임 확인을 먼저 수행한다.**

#### D. `DungeonStateManager.ClearStates()` 호출처가 없음

메서드는 존재하나 프로젝트 전체에서 호출하는 곳이 0개다.
던전 오브젝트(문·장치)의 개폐 상태가 다음 판에 잔존한다.

#### E. `GameManager._rebirthRoutine` 누수

부활 5초 대기 중 판이 종료되면 코루틴이 살아남아 다음 판 시작 직후
`Player.Spawn()`과 `CurrentState = Playing`을 실행할 수 있다.

#### F. `MonsterManager` 참조 잔존

`monsters` 리스트에 파괴된 오브젝트가 null로 남고, `Player`도 이전 판의 파괴된 참조를 가리킨다.

#### G. `PoolManager.ClearPools()`가 주석 처리됨

`GameManager.ExitGame()`에 `// PoolManager.Instance.ClearPools();   // ?? 왜 풀 리셋하면 오류가 나지?`
라는 주석이 있다. 풀 오브젝트는 `PoolManager`(Persistent) 하위에 있어 씬 언로드에도 살아남으므로
**풀 자체는 유지하는 것이 맞다.** 다만 대여 중(active) 상태로 씬과 함께 파괴된 오브젝트가
풀 밖에서 사라지면 다음 판에 null이 튄다. 언로드 **전에** 회수해야 한다.

### 7.2 해결 설계

각 매니저에 `ResetSession()`을 두고, `GameManager`가 순서대로 호출하는 단일 진입점을 만든다.

```csharp
public async void ReturnToTitleFromDemo()
{
    // 1. 씬 언로드 "전에" 풀 오브젝트 회수  ← 순서가 핵심 (결함 G)
    MonsterManager.Instance.ResetSession();     // ReleaseAllMonsters + Player = null

    // 2. 진행 중 코루틴 중단 및 참조 해제 (결함 E)
    if (_rebirthRoutine != null) { StopCoroutine(_rebirthRoutine); _rebirthRoutine = null; }

    // 3. 씬 언로드
    await DungeonManager.Instance.UnloadAllStage();
    await UnloadPlayerScene();

    // 4. 세션 오브젝트 일괄 파괴 (결함 C) — 씬 언로드 "이후"
    DestroySessionObjects();                    // Player / 카메라 / 미니맵 등 (§7.3)

    // 5. 매니저 상태 리셋 (결함 A, B, D)
    DungeonManager.Instance.ResetSession();     // _isInit=false, LoadedStages.Clear(), index=0
    DungeonStateManager.Instance.ClearStates();
    TutorialModeContext.Reset();

    // 5. 참조 해제
    Player = null; FollowCamera = null; MinimapObject = null;
    PlayMode = EPlayMode.Normal;

    // 6. 타이틀 복귀
    EnterTitle();
}
```

**`Scene_UI`는 언로드하지 않는다.** `EnterTitle()`이 호출하는 `LoadGUI()`는
`SceneController.LoadSceneAdditive`의 중복 로드 방지에 의해 no-op이 되며,
`GUIManager`가 초기화된 상태를 그대로 유지하는 것이 안전하다.

**`PoolManager`는 리셋하지 않는다.** Persistent 하위에 있어 씬 언로드의 영향을 받지 않는다.

### 7.3 결함 C의 수정 방침 — 세션 오브젝트 레지스트리

§7.1-C의 전제에 따라 **씬 소속과 Active Scene은 일절 건드리지 않는다.**
런타임에 생성된 오브젝트를 명시적으로 등록해두고 세션 종료 시 직접 파괴한다.

```csharp
// GameManager
private readonly List<GameObject> _sessionObjects = new();

/// 판(session) 단위로 생성되어 타이틀 복귀 시 파괴되어야 할 오브젝트를 등록한다.
/// Active Scene이 Scene_Persistent로 고정되어 있어 씬 언로드로는 정리되지 않으므로,
/// 생성 측이 직접 등록하고 ReturnToTitleFromDemo()가 일괄 파괴한다.
public void RegisterSessionObject(GameObject go)
{
    if (go != null) _sessionObjects.Add(go);
}

private void DestroySessionObjects()
{
    foreach (GameObject go in _sessionObjects)
        if (go != null) Destroy(go);

    _sessionObjects.Clear();
}
```

등록 지점:

| 대상 | 수정 |
|---|---|
| `InitPlayer.Init()` | `createdObj`의 모든 값을 `RegisterSessionObject()`로 등록 (Player, Minimap, Volume, Navi, FollowCamera, MinimapCamera, WorldMapCamera, LowHp) |
| `DungeonManager.LoadMiniMap()` | `Instantiate` 결과를 `RegisterSessionObject()`로 등록 |

**호출 순서**: `UnloadPlayerScene()`이 `FollowAudioListener.Unload()`를 호출하므로,
`DestroySessionObjects()`는 **씬 언로드 이후**에 실행한다 (§7.2의 4단계).

`InitPlayer`는 `Awake()` 끝에서 `Destroy(gameObject)`로 자신을 파괴하므로 로더 오브젝트
자체는 등록 대상이 아니다.

이 방식의 이점:

- 씬 소속·Active Scene을 변경하지 않아 라이팅/컬링 제약을 완전히 지킨다
- `InitPlayer`(로더 씬 거주)와 `DungeonManager`(Persistent 거주)를 **하나의 메커니즘**으로 처리한다
- 이후 세션 단위 생성물이 추가되어도 등록 한 줄로 끝난다

> **부수 수정**: `GameManager.UnloadPlayerScene()`은 `MinimapObject.GetComponent<...>()`를
> null 검사 없이 호출한다. 반복 실행 시 NRE 원인이 되므로 null 가드를 추가한다.

### 7.4 범위에 대한 판단

§7의 수정은 본편 매니저를 건드리므로 **본편 회귀 테스트가 필요하다.**

범위를 줄이는 대안으로 "데모 경로에서만 타는 `ResetSession`을 만들고 본편 `ExitGame`은
손대지 않는" 방법이 있으나, 결함 A~C는 본편의 반복 플레이에도 동일하게 존재하며
데모 경로가 이를 우회할 방법이 없다. **공통 경로로 수정하고 본편을 함께 검증한다.**

### 7.5 관찰되었으나 이번 범위 밖인 항목

`UI_Credits.Update()`가 `targetPosition` 도달 후 **매 프레임** `GameManager.EnterTitle()`을
호출한다. `EnterTitle()`은 `async void`이므로 비동기 작업이 중첩 실행된다.
크레딧은 데모 플로우에 포함되지 않으므로 이번 범위에서 제외하되, 별도로 처리할 가치가 있다.

---

## 8. 튜토리얼 씬 규약 (맵 작업 병렬 진행용)

맵 디자인은 아직 논의되지 않았으나, 아래 규약만 확정되면 맵 작업을 병렬로 진행할 수 있다.
임시 맵(회색 박스)으로 먼저 검증한 뒤 실제 아트를 교체한다.

### 필수 포함 요소

| # | 요소 | 비고 |
|---|---|---|
| 1 | **NavMesh 베이크** | 없으면 아몬이 추적하지 않는다 |
| 2 | 초기화 컴포넌트 | `Init1stGameScene` 계열. `DungeonManager.SetStartPosition()` / `RestartPosition` 주입 |
| 3 | **`arenaAnchor` Transform** | 아레나 중심. 아몬 스킬의 워프/스폰 좌표 기준. 누락 시 월드 원점 기준으로 폴백되어 스킬이 엉뚱한 위치에서 발동 |
| 4 | `playerTeleportPoint` Transform | 보스전 시작 시 플레이어 배치 지점 |
| 5 | `playerRespawnPoint` Transform | `AmonEndPhase()` 대상 |
| 6 | 아몬 2페이즈 프리팹 인스턴스 | `isEnabled = false` 상태로 배치 |
| 7 | `TutorialDirector` + 조건 컴포넌트 | 스텝별 트리거 볼륨 포함 |
| 8 | 튜토리얼 구간 ↔ 아레나 구간 분리 | 문 또는 차단막. 튜토리얼 중 보스 조우 방지 |
| 9 | **Addressables 등록** | 주소 `Scene_Tutorial` |

### 구간 구성 제안

```
[시작 지점] → [이동 튜토리얼] → [공격 튜토리얼 + 더미] → [차단문] → [보스 아레나]
```

---

## 9. 구현 순서

의존 관계를 고려한 권장 순서다. 각 단계마다 검증 가능한 상태를 유지한다.

| 단계 | 작업 | 검증 |
|---|---|---|
| 0 | §7 결함 C 런타임 확인 | 본편 진입 후 `Scene_Persistent` 하위 관찰 / `PlayerController` 인스턴스 수 |
| 1 | 모드 플래그 + 스테이지 세트 분기 + **임시 빈 튜토리얼 씬** + 임시 "타이틀로" 디버그 버튼 | 타이틀 → 데모 → 타이틀 **짧은 왕복 루프 확보** |
| 2 | §7 세션 리셋 구현 | 1단계 루프로 3회 이상 왕복 → 매회 정상 로드 |
| 3 | 임시 맵 + 아몬 2페이즈 단독 기동 | §5 체크리스트 5항목 전부 확인 |
| 4 | 스탯 배수 오버라이드 | 배수가 보스 HP에 반영되는지, 재초기화 시 중첩되지 않는지 |
| 5 | 결과 화면 + 정식 타이틀 복귀 | 임시 디버그 버튼 제거, 데모 → 결과 → 타이틀 왕복 3회 |
| 6 | `TutorialDirector` + 스텝 조건 | 전체 플로우 통과 |
| 7 | 밸런싱 (프롤로그 실측 → 배수 역산) | 5분 예산 실측 |
| 8 | (선택) 소프트 타임아웃 | 일정 여유 시에만 |

**1·2단계 순서의 근거**: 세션 리셋을 검증하려면 "판을 끝내고 타이틀로 돌아가는" 짧은 루프가
먼저 있어야 한다. 현재 본편에는 그런 경로가 에필로그 → 크레딧 → `EnterTitle()`밖에 없어
1회 검증에 10분 이상이 걸린다. 팀이 "재플레이 QA가 번거로워 테스트를 거의 못 했다"고 한
상황이 정확히 이것이다.

따라서 **1단계에서 임시 빈 씬 + 임시 복귀 버튼으로 30초짜리 왕복 루프를 먼저 만들고**,
그 루프 위에서 2단계 세션 리셋을 검증한다. 이 임시 루프는 5단계에서 정식 결과 화면으로
교체되며, 그 이후로는 본편 회귀 테스트에도 활용할 수 있다.

---

## 10. 미결 사항

- 튜토리얼 스텝 구성 (어떤 조작을 몇 개나 가르칠지) — 맵 구조 확정 후 결정
- `BossHealthMultiplier` / `BossDamageMultiplier` 실측 값 — 7단계에서 결정
- 결과 화면 문구 및 디자인
- 소프트 타임아웃 채택 여부 — 일정 여유에 따라 결정
