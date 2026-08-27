using Monster.AI.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Test.Skills
{
    // # =====================================================================
    // # 스킬: Lockdown (영혼 감옥) / skillID 4016 / 아몬 2페이즈
    // # 담당 스크립트: AmonLockdown.cs (SkillData 상속)
    // # ---------------------------------------------------------------------
    // # [Casting 단계 - 준비]
    // #   1. 보스를 warpPosition(공중 위치)으로 워프
    // #   2. "isCharging" 애니메이션 ON, 본체 위에 기 모으기 이펙트 생성
    // #   3. 필드 랜덤 위치에 안전지대(safeZonePrefab) 생성
    // #   4. castTime 동안 monsterSpawnInterval 간격으로 monsterPrefabs 중
    // #      랜덤 몬스터를 필드 랜덤 위치(X,Z 범위)에 지속 스폰하여 _targets에 등록
    //     #
    // # [Activate 단계 - 폭발/종료]
    // #   1. 기 모으기 이펙트 제거, 폭발 이펙트 생성(2초 후 제거), 안전지대 제거
    // #   2. "isCharging" 애니메이션 OFF
    // #   3. 플레이어에게 데미지 적용 후 무적(Invincibility) 상태 해제
    // #      → 안전지대에서 얻은 무적이면 폭발 데미지를 무효화하는 구조
    // #   4. 스폰된 모든 몬스터를 일괄 제거(대량 데미지)
    // #   5. 보스를 defaultPosition으로 복귀
    //     #
    // # [참고] Activate의 플레이어 폭발 데미지는 에셋의 damage 필드(SkillData) 값을 사용한다.
    // # =====================================================================
    [CreateAssetMenu(fileName = "Lockdown", menuName = "MonsterSkills/Amon_Phase2/Lockdown")]
    public class AmonLockdown : SkillData
    {
        private static readonly int IsCharging = Animator.StringToHash("isCharging");

        [Header("그 외 스킬 정보")]
        [SerializeField] private Vector3 defaultPosition;
        [Tooltip("캐스팅 중 보스가 이동할 위치. 높이(Y)는 아래 warpHeight 값을 사용하고, 여기서는 X/Z만 사용한다.")]
        [SerializeField] private Vector3 warpPosition;
        [Tooltip("캐스팅 중 보스가 떠오르는 높이(Y). 너무 높으면 카메라에 잡기 어려우므로 이 값으로 조절한다.")]
        [SerializeField] private float warpHeight = 5.0f;
        [SerializeField] private GameObject chargeEffectPrefab;
        [SerializeField] private Vector3 chargeEffectRotation;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private Vector3 effectSpawnOffset;
        [SerializeField] private List<GameObject> monsterPrefabs = new();
        [Tooltip("몬스터/안전지대 스폰 영역의 중심 (앵커 로컬 기준). Y는 스폰 높이로 사용한다.")]
        [SerializeField] private Vector3 spawnAreaCenter;
        [Tooltip("스폰 영역의 크기. X = 가로(X축) 너비, Y = 세로(Z축) 깊이. 중심 ± 크기/2 범위에 랜덤 스폰된다.")]
        [SerializeField] private Vector2 spawnAreaSize = new(10f, 10f);
        [SerializeField] private float monsterSpawnInterval = 1.0f;
        [SerializeField] private GameObject safeZonePrefab;
        private GameObject _chargeEffect;
        private GameObject _safeZone;
        private readonly List<IDamagable> _targets = new();

        // 매직 넘버 상수화 (플레이어 폭발 데미지는 에셋의 damage 필드를 사용)
        private const float MonsterCleanupDamage = 10000.0f;
        private const float ExplosionEffectLifetime = 2.0f;
        // 몬스터 일괄 정리는 레이어와 무관하게 항상 적중하도록 모든 레이어를 대상으로 한다.
        private const int AllLayers = ~0;

        // ReSharper disable Unity.PerformanceAnalysis
        public override IEnumerator Activate(Blackboard data)
        {
            Debug.Log("[Amon Phase 2] 영혼 감옥 시작");

            // 5.기 모으기 이펙트 제거 및 폭발 이펙트 생성
            if (_chargeEffect is not null)
            {
                Utils.Destroy(_chargeEffect);
            }
            Utils.Destroy(Utils.Instantiate(explosionEffectPrefab, data.Agent.transform.position + effectSpawnOffset, Quaternion.identity), ExplosionEffectLifetime);

            // 6. 플레이어 및 스폰된 몬스터 전체에게 데미지 처리 (임의 함수 호출 예시)
            data.AnimatorParameterSetter.Animator.SetBool(IsCharging, false);

            // 플레이어가 폭발 순간 안전지대 안에 있으면 피해를 면제한다.
            // 무적 플래그/안전지대 해제 타이밍에 의존하지 않도록, 점유 여부를 직접 질의한다.
            GameObject playerObject = Managers.MonsterManager.Instance.Player;
            if (playerObject != null && playerObject.TryGetComponent(out IDamagable target))
            {
                bool sheltered = _safeZone != null
                    && _safeZone.TryGetComponent(out SafeZoneObject zone)
                    && playerObject.TryGetComponent(out PlayerController pc)
                    && zone.IsProtecting(pc);

                // 데미지는 에셋의 damage 필드(SkillData) 값을 사용. 플레이어 본인 레이어로 마스크를 구성한다.
                if (!sheltered)
                {
                    target.ApplyDamage(damage, 1 << playerObject.layer);
                }
            }

            // 안전지대는 플레이어 데미지/무적 처리가 끝난 뒤 파괴한다.
            // (먼저 파괴하면 OnTriggerExit로 무적이 풀려, 안전지대에 있던 플레이어도 폭발 데미지를 받을 수 있음)
            Utils.Destroy(_safeZone, 0.5f);

            // 기믹 종료 후 스폰된 몬스터 일괄 제거 (파괴되어 null이 된 참조는 건너뜀)
            foreach (IDamagable t in _targets)
            {
                t?.ApplyDamage(MonsterCleanupDamage, AllLayers);
            }

            data.NavMeshAgent.Warp(AnchorToWorld(data.ArenaAnchor, defaultPosition));

            // 공유 ScriptableObject 인스턴스이므로 다음 시전을 위해 런타임 상태를 초기화
            ResetRuntimeState();

            Debug.Log("[Amon Phase 2] 영혼 감옥 종료");
            yield break;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public override IEnumerator Casting(Blackboard data)
        {
            Debug.Log("[Amon Phase 2] 영혼 감옥 준비");

            // 파훼 방법을 모르면 대처할 수 없는 패턴이므로 시전 시작과 함께 안내한다.
            ShowCastNotice();

            // 공유 인스턴스이므로 이전 시전에서 남은 참조를 비우고 시작 (몬스터/이펙트 누적 방지)
            ResetRuntimeState();

            // 모든 워프/스폰 좌표는 아레나 앵커 로컬 기준으로 해석한다 (앵커 미할당 시 월드 폴백)
            Transform anchor = data.ArenaAnchor;
            if (!anchor)
            {
                Debug.LogWarning("[Amon Phase 2] ArenaAnchor가 지정되지 않아 좌표를 월드 기준으로 처리한다. Blackboard에 앵커를 할당하라.");
            }

            // 1. 보스 특정 위치로 이동 (높이는 warpHeight로 조절, X/Z는 warpPosition 사용)
            Vector3 warpTarget = new(warpPosition.x, warpHeight, warpPosition.z);
            data.NavMeshAgent.Warp(AnchorToWorld(anchor, warpTarget));

            // 2. 기 모으기 애니메이션 재생
            data.AnimatorParameterSetter.Animator.SetBool(IsCharging, true);

            // 3. 기 모으기 이펙트 생성 (본체 위)
            _chargeEffect = Utils.Instantiate(chargeEffectPrefab, data.Agent.transform.position + effectSpawnOffset, Quaternion.Euler(chargeEffectRotation), data.Agent.transform);

            _safeZone = Utils.Instantiate(safeZonePrefab, GetRandomSpawnPosition(anchor), Quaternion.identity);
            
            float elapsed = 0f;
            float spawnTimer = 0f;
            // 4. 캐스팅 시간 동안 몬스터 지속 생성 및 대기
            while (elapsed < castTime)
            {
                elapsed += Time.deltaTime;
                spawnTimer += Time.deltaTime;

                // 스폰할 프리팹이 등록된 경우에만 진행 (빈 리스트일 때 인덱스 예외 방지)
                if (spawnTimer >= monsterSpawnInterval && monsterPrefabs.Count > 0)
                {
                    spawnTimer = 0f;

                    // 필드 내 랜덤 위치 계산 (앵커 로컬 기준 → 월드 변환)
                    int rand = Random.Range(0, monsterPrefabs.Count);
                    GameObject go = Utils.Instantiate(monsterPrefabs[rand], GetRandomSpawnPosition(anchor), Quaternion.identity);
                    if (go.TryGetComponent(out IDamagable damageable))
                    {
                        _targets.Add(damageable);
                    }
                }

                yield return null;
            }
        }

        // 시전 중 강제 중단(피격 등) 시 호출. Activate가 실행되지 못해 남는
        // 이펙트/안전지대/스폰 몬스터를 정리하고 애니메이션 플래그를 복구한다.
        // ReSharper disable Unity.PerformanceAnalysis
        public override void OnInterrupt(Blackboard data)
        {
            data.AnimatorParameterSetter?.Animator?.SetBool(IsCharging, false);

            Utils.Destroy(_chargeEffect);
            Utils.Destroy(_safeZone, 0.5f);

            // 스폰된 몬스터도 함께 제거 (정상 종료 시 Activate와 동일한 처리)
            foreach (IDamagable t in _targets)
            {
                t?.ApplyDamage(MonsterCleanupDamage, AllLayers);
            }

            // 보스를 기본 위치로 복귀시킨다. (Casting에서 warpPosition(공중)으로 이동했으므로,
            // 복귀하지 않으면 중단 시 보스가 공중에 멈춘 채로 남는다 — Activate 93줄과 동일한 처리)
            if (data.NavMeshAgent != null)
            {
                data.NavMeshAgent.Warp(AnchorToWorld(data.ArenaAnchor, defaultPosition));
            }

            ResetRuntimeState();
        }

        // 앵커가 지정돼 있으면 로컬 좌표를 월드로 변환하고, 없으면 월드 좌표로 간주(폴백).
        // 아레나가 원점이 아닌 곳/회전된 상태로 배치돼도 데이터를 그대로 재사용할 수 있게 한다.
        private static Vector3 AnchorToWorld(Transform anchor, Vector3 localPoint)
        {
            return anchor != null ? anchor.TransformPoint(localPoint) : localPoint;
        }

        // 스폰 영역(spawnAreaCenter 중심, spawnAreaSize 크기) 안의 랜덤 위치를 월드 좌표로 반환한다.
        // X/Z는 중심 ± 크기/2 범위에서 무작위, Y는 중심 높이로 고정한다.
        private Vector3 GetRandomSpawnPosition(Transform anchor)
        {
            float halfWidth = spawnAreaSize.x * 0.5f;
            float halfDepth = spawnAreaSize.y * 0.5f;

            float x = spawnAreaCenter.x + Random.Range(-halfWidth, halfWidth);
            float z = spawnAreaCenter.z + Random.Range(-halfDepth, halfDepth);

            return AnchorToWorld(anchor, new Vector3(x, spawnAreaCenter.y, z));
        }

        // 시전 종료/시작 시 런타임 상태를 초기화. 공유 ScriptableObject 인스턴스가
        // 이전 시전의 몬스터/이펙트 참조를 누적하지 않도록 보장한다.
        private void ResetRuntimeState()
        {
            _targets.Clear();
            _chargeEffect = null;
            _safeZone = null;
        }
    }
}
