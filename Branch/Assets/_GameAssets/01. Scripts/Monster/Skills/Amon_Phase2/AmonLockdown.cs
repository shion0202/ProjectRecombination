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
// # [주의] Activate의 플레이어 데미지는 코드에 500으로 하드코딩되어 있어
// #        아래 damage(400) 값과 일치하지 않음.
// # =====================================================================
    [CreateAssetMenu(fileName = "Lockdown", menuName = "MonsterSkills/Amon_Phase2/Lockdown")]
    public class AmonLockdown : SkillData
    {
        [Header("그 외 스킬 정보")]
        [SerializeField] private Vector3 defaultPosition;
        [SerializeField] private Vector3 warpPosition;
        [SerializeField] private GameObject chargeEffectPrefab;
        [SerializeField] private Vector3 chargeEffectRotation;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private Vector3 effectSpawnOffset;
        [SerializeField] private List<GameObject> monsterPrefabs = new();
        [SerializeField] private Vector2 spawnTargetPositionX;
        [SerializeField] private Vector2 spawnTargetPositionZ;
        [SerializeField] private float spawnTargetPositionY;
        [SerializeField] private float monsterSpawnInterval = 1.0f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private GameObject safeZonePrefab;
        private GameObject _chargeEffect;
        private GameObject _safeZone;
        private List<IDamagable> _targets = new();

        // 매직 넘버 상수화 (플레이어 폭발 데미지는 에셋의 damage 필드를 사용)
        private const float MonsterCleanupDamage = 10000.0f;
        private const float ExplosionEffectLifetime = 2.0f;
        private const float SafeZoneSpawnHeightOffset = 10.0f;

        public override IEnumerator Activate(Blackboard data)
        {
            Debug.Log("[Amon Phase 2] 영혼 감옥 시작");

            // 5.기 모으기 이펙트 제거 및 폭발 이펙트 생성
            if (_chargeEffect != null)
            {
                Utils.Destroy(_chargeEffect);
            }
            Utils.Destroy(Utils.Instantiate(explosionEffectPrefab, data.Agent.transform.position + effectSpawnOffset, Quaternion.identity), ExplosionEffectLifetime);
            Utils.Destroy(_safeZone);

            // 6. 플레이어 및 스폰된 몬스터 전체에게 데미지 처리 (임의 함수 호출 예시)
            data.AnimatorParameterSetter.Animator.SetBool("isCharging", false);

            // 플레이어 오브젝트는 한 번만 조회해 IDamagable / PlayerController를 재사용
            GameObject playerObject = Managers.MonsterManager.Instance.Player;
            if (playerObject != null)
            {
                // 폭발 데미지 적용 후 무적 해제 (안전지대 무적으로 데미지를 무효화하는 구조이므로 순서 유지)
                // 데미지는 에셋의 damage 필드(SkillData) 값을 사용
                if (playerObject.TryGetComponent(out IDamagable target))
                {
                    target.ApplyDamage(damage, targetMask);
                }
                if (playerObject.TryGetComponent(out PlayerController player))
                {
                    player.SetPlayerState(EPlayerState.Invincibility, false);
                }
            }

            // 기믹 종료 후 스폰된 몬스터 일괄 제거 (파괴되어 null이 된 참조는 건너뜀)
            for (int i = 0; i < _targets.Count; ++i)
            {
                _targets[i]?.ApplyDamage(MonsterCleanupDamage, targetMask);
            }

            data.NavMeshAgent.Warp(AnchorToWorld(data.ArenaAnchor, defaultPosition));

            // 공유 ScriptableObject 인스턴스이므로 다음 시전을 위해 런타임 상태를 초기화
            ResetRuntimeState();

            Debug.Log("[Amon Phase 2] 영혼 감옥 종료");
            yield break;
        }

        public override IEnumerator Casting(Blackboard data)
        {
            Debug.Log("[Amon Phase 2] 영혼 감옥 준비");

            // 공유 인스턴스이므로 이전 시전에서 남은 참조를 비우고 시작 (몬스터/이펙트 누적 방지)
            ResetRuntimeState();

            // 모든 워프/스폰 좌표는 아레나 앵커 로컬 기준으로 해석한다 (앵커 미할당 시 월드 폴백)
            Transform anchor = data.ArenaAnchor;
            if (anchor == null)
            {
                Debug.LogWarning("[Amon Phase 2] ArenaAnchor가 지정되지 않아 좌표를 월드 기준으로 처리한다. Blackboard에 앵커를 할당하라.");
            }

            // 1. 보스 특정 위치로 이동
            data.NavMeshAgent.Warp(AnchorToWorld(anchor, warpPosition));

            // 2. 기 모으기 애니메이션 재생
            data.AnimatorParameterSetter.Animator.SetBool("isCharging", true);

            // 3. 기 모으기 이펙트 생성 (본체 위)
            _chargeEffect = Utils.Instantiate(chargeEffectPrefab, data.Agent.transform.position + effectSpawnOffset, Quaternion.Euler(chargeEffectRotation), data.Agent.transform);

            float x = UnityEngine.Random.Range(spawnTargetPositionX.x, spawnTargetPositionX.y);
            float z = UnityEngine.Random.Range(spawnTargetPositionZ.x, spawnTargetPositionZ.y);
            Vector3 spawnPos = AnchorToWorld(anchor, new Vector3(x, spawnTargetPositionY + SafeZoneSpawnHeightOffset, z));
            _safeZone = Utils.Instantiate(safeZonePrefab, spawnPos, Quaternion.identity);

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
                    x = UnityEngine.Random.Range(spawnTargetPositionX.x, spawnTargetPositionX.y);
                    z = UnityEngine.Random.Range(spawnTargetPositionZ.x, spawnTargetPositionZ.y);
                    spawnPos = AnchorToWorld(anchor, new Vector3(x, spawnTargetPositionY, z)); // 높이는 0 고정, 필요시 수정

                    int rand = UnityEngine.Random.Range(0, monsterPrefabs.Count);
                    GameObject go = Utils.Instantiate(monsterPrefabs[rand], spawnPos, Quaternion.identity);
                    if (go.TryGetComponent(out IDamagable damagable))
                    {
                        _targets.Add(damagable);
                    }
                }

                yield return null;
            }
        }

        // 시전 중 강제 중단(피격 등) 시 호출. Activate가 실행되지 못해 남는
        // 이펙트/안전지대/스폰 몬스터를 정리하고 애니메이션 플래그를 복구한다.
        public override void OnInterrupt(Blackboard data)
        {
            data.AnimatorParameterSetter?.Animator?.SetBool("isCharging", false);

            Utils.Destroy(_chargeEffect);
            Utils.Destroy(_safeZone);

            // 스폰된 몬스터도 함께 제거 (정상 종료 시 Activate와 동일한 처리)
            for (int i = 0; i < _targets.Count; ++i)
            {
                _targets[i]?.ApplyDamage(MonsterCleanupDamage, targetMask);
            }

            ResetRuntimeState();
        }

        // 앵커가 지정돼 있으면 로컬 좌표를 월드로 변환하고, 없으면 월드 좌표로 간주(폴백).
        // 아레나가 원점이 아닌 곳/회전된 상태로 배치돼도 데이터를 그대로 재사용할 수 있게 한다.
        private static Vector3 AnchorToWorld(Transform anchor, Vector3 localPoint)
        {
            return anchor != null ? anchor.TransformPoint(localPoint) : localPoint;
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
