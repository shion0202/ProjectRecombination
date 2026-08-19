using Monster.AI.Blackboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Test.Skills
{
    /// <summary>
    /// 스킬 이름: 돌진 (2페이즈)
    /// - 캐스팅: 1.5초
    /// - 효과: 대상을 향해 돌진, 적중 시 넉백 및 대미지
    /// - 예외처리1: 캐스팅 중 스턴, 넉백, 이동 불가 상태가 되면 스킬 취소
    /// - 예외처리2: 대상과 거리가 너무 멀 경우 스킬 취소
    /// - 예외처리3: 돌진 중 장애물에 부딪히면 돌진 취소
    /// </summary>
    
    [CreateAssetMenu(fileName = "ChargingLaser", menuName = "MonsterSkills/Executioner/ChargingLaser")]
    public class ExeChargingLaser : SkillData
    {
        private static readonly int IsLaser = Animator.StringToHash("isLaser");
        private const string AimPointName = "TargetPos";

        [Header("그 외 스킬 정보")]
        [SerializeField] private GameObject laserPrefab;
        [SerializeField] private Vector3 laserOffset;
        [SerializeField] private float attackDuration;
        [Tooltip("캐스팅 중 몸체가 대상을 따라 도는 회전 속도")]
        [SerializeField] private float rotateSpeed = 2.0f;
        [Tooltip("발사 중 빔이 대상을 따라 도는 최대 각속도(도/초). 낮을수록 회피가 쉬워진다.")]
        [SerializeField] private float fireRotateSpeed = 60.0f;

        [Header("히트 판정 (DoT)")]
        [Tooltip("레이저 빔의 최대 사거리(Raycast 길이)")]
        [SerializeField] private float maxLength = 50.0f;
        [Tooltip("빔 두께(SphereCast 반경). 레이저 VFX 굵기에 맞춰 조절한다.")]
        [SerializeField] private float beamRadius = 0.5f;
        [Tooltip("대미지를 입힐 대상 레이어 (예: Player)")]
        [SerializeField] private LayerMask targetMask;
        [Tooltip("빔을 막는 장애물 레이어 (이 레이어에 먼저 맞으면 대상에게 닿지 않음)")]
        [SerializeField] private LayerMask obstacleMask;
        [Tooltip("DoT 틱 간격(초). damage 값은 초당 대미지로 취급된다.")]
        [SerializeField] private float damageInterval = 0.1f;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip chargingLaserAudioClip;

        public override IEnumerator Activate(Blackboard data)
        {
            Debug.Log("[Executioner] Charging Laser 시작");

            // 빔은 플레이어 자식 TargetPos(가슴 높이)를 3D로 추적하되, 회전 각속도를 fireRotateSpeed로 제한한다.
            // 따라서 정확히 가슴을 겨냥하면서도 플레이어가 충분히 빠르게 움직이면 회피할 수 있다. (즉발 조준 아님)

            // SkillData는 공유 ScriptableObject 인스턴스이므로, 시전마다 만드는 런타임 오브젝트는
            // 필드가 아니라 지역 변수로만 관리한다 (다중 몬스터 간 상태 오염 방지).
            GameObject shootObject = null;
            GameObject laser = null;
            try
            {
                shootObject = new GameObject("ExeChargingLaser_ShootPoint");
                Transform shootPoint = shootObject.transform;
                shootPoint.SetParent(data.Agent.transform);
                shootPoint.localPosition = laserOffset;
                shootPoint.localRotation = Quaternion.identity;

                laser = Utils.Instantiate(laserPrefab, shootPoint);
                laser.transform.localPosition = Vector3.zero;
                laser.transform.localRotation = Quaternion.identity;

                data.AudioSource.PlayOneShot(chargingLaserAudioClip);
                data.AnimatorParameterSetter.Animator.SetBool(IsLaser, true);

                // 조준점(플레이어 자식 TargetPos)과 빔 회전을 몸체와 분리해 독립적으로 각속도를 제한한다.
                Transform aimPoint = ResolveAimPoint(data.Target);
                Quaternion beamRotation = laser.transform.rotation; // 시작 시 몸체 정면
                
                float elapsed = 0f;
                float damageTimer = 0f;
                while (elapsed < attackDuration)
                {
                    // 시전 도중 타깃이 사라지면 즉시 종료 (NRE 방지)
                    if (data.Target == null) break;

                    // 타깃이 바뀌었거나(리스폰) 조준점을 잃으면 다시 탐색
                    if (aimPoint == null || !aimPoint.IsChildOf(data.Target.transform))
                    {
                        aimPoint = ResolveAimPoint(data.Target);
                    }

                    // 몸체는 조준점 방향으로 '수평(yaw)'만 제한 회전 (연출용, 몸체가 기울지 않도록)
                    Vector3 bodyDir = aimPoint.position - data.Agent.transform.position;
                    bodyDir.y = 0;
                    if (bodyDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion bodyTarget = Quaternion.LookRotation(bodyDir);
                        data.Agent.transform.rotation = Quaternion.RotateTowards(
                            data.Agent.transform.rotation, bodyTarget, fireRotateSpeed * Time.deltaTime);
                    }

                    // 빔(=실제 조준/판정)은 TargetPos를 3D로 추적하되 fireRotateSpeed로 회전 제한한다.
                    // 몸체 회전 이후 마지막에 월드 회전을 강제해 부모(몸체) 회전에 끌려가지 않게 한다.
                    Vector3 beamDir = aimPoint.position - shootPoint.position;
                    if (beamDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion beamTarget = Quaternion.LookRotation(beamDir);
                        beamRotation = Quaternion.RotateTowards(beamRotation, beamTarget, fireRotateSpeed * Time.deltaTime);
                        laser.transform.rotation = beamRotation;
                    }

                    // DoT 히트 판정: 틱 간격마다 빔 방향으로 SphereCast
                    damageTimer += Time.deltaTime;
                    if (damageTimer >= damageInterval)
                    {
                        ApplyBeamDamage(laser.transform, damageTimer);
                        damageTimer = 0f;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            finally
            {
                // 정상 종료/예외 전파 모두에서 애니메이션 플래그와 생성물을 정리한다.
                if (data.AnimatorParameterSetter?.Animator != null)
                {
                    data.AnimatorParameterSetter.Animator.SetBool(IsLaser, false);
                }
                if (laser != null) Utils.Destroy(laser);
                if (shootObject != null) Destroy(shootObject);
            }

            Debug.Log("[Executioner] Charging Laser 종료");
        }

        /// <summary>
        /// 빔 시작점(beam)에서 forward 방향으로 SphereCast하여, 가장 먼저 맞은 것이 대상이면 DoT 대미지를 적용한다.
        /// 장애물(obstacleMask)에 먼저 맞으면 빔이 막힌 것으로 보고 대미지를 주지 않는다.
        /// </summary>
        private void ApplyBeamDamage(Transform beam, float tickTime)
        {
            // 대상 + 장애물을 함께 검사하여, 둘 중 더 가까운 쪽이 먼저 맞도록 한다.
            // 빔 두께를 반영하기 위해 단일 Raycast 대신 beamRadius 기반 SphereCast를 사용한다.
            int hitMask = targetMask | obstacleMask;
            if (!Physics.SphereCast(beam.position, beamRadius, beam.forward, out RaycastHit hit, maxLength, hitMask))
            {
                return;
            }

            // 먼저 맞은 것이 대상 레이어가 아니면(=장애물) 빔이 막힌 것이므로 무시
            if ((targetMask.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                return;
            }

            IDamagable target = hit.collider.GetComponent<IDamagable>()
                                ?? hit.collider.GetComponentInParent<IDamagable>();
            // null 이거나 Component가 아니면 무시
            if (target is not Component targetComponent)
            {
                return;
            }

            // damage = 초당 대미지. tickTime(실제 경과 시간)을 unitOfTime으로 넘겨
            // 방어력이 시간 구간에 비례 적용되도록 한다. (ShoulderLaser와 동일한 규약)
            // 레이캐스트로 이미 대상을 검증했으므로, IDamagable '본체'의 레이어로 마스크를 구성해
            // ApplyDamage 내부 레이어 재검사(콜라이더 레이어 ≠ 본체 레이어)로 데미지가 누락되는 것을 막는다.
            target.ApplyDamage(damage * tickTime, 1 << targetComponent.gameObject.layer, tickTime, 0.0f);
        }

        // 플레이어(타깃)의 자식 "TargetPos"를 조준점으로 사용한다. 없으면 타깃 루트로 폴백한다.
        private static Transform ResolveAimPoint(GameObject target)
        {
            if (target == null) return null;
            Transform found = FindDeepChild(target.transform, AimPointName);
            return found != null ? found : target.transform;
        }

        // 이름이 일치하는 자손 Transform을 재귀로 탐색(할당 없음). 없으면 null.
        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent.name == childName) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindDeepChild(parent.GetChild(i), childName);
                if (found != null) return found;
            }
            return null;
        }

        public override IEnumerator Casting(Blackboard data)
        {
            Debug.Log("[Executioner] Charging Laser 준비");

            // To-do: 레이저 패턴임을 식별하기 쉽도록, laserOffset 위치에 차지 또는 발광 이펙트 생성 필요
            // 1. 캐스팅 중 레이저 본이 느리게 플레이어를 따라감
            // 애니메이션이 적용된 상태에서 본 회전 구현이 어려워, transform 전체를 회전시키도록 구현한 상태
            float elapsed = 0f;
            while (elapsed < castTime)
            {
                if (data.Target != null)
                {
                    Vector3 lookDir = data.Target.transform.position - data.Agent.transform.position;
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0.001f)
                    {
                        Quaternion now = data.Agent.transform.rotation;
                        Quaternion target = Quaternion.LookRotation(lookDir);
                        data.Agent.transform.rotation = Quaternion.Slerp(now, target, Time.deltaTime * rotateSpeed);
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
