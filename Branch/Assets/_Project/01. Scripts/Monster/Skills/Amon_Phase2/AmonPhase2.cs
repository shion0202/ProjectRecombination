using Monster.AI.Blackboard;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

// 스킬 직전에 움직임을 멈추고 캐릭터를 바라본 뒤
// 날개가 15도 뒤로 꺾인 채 (UpMove)
// 빠르게 캐릭터를 향해 돌진함 (RunMove)

public class AmonPhase2 : MonoBehaviour
{
    [Header("공통 값 1")]
    [SerializeField] private AmonMeleeCollision meleeCollision;     // 근접 공격 범위 (Collision을 사용하여 On/Off하는 방식)

    [Header("공통 값 2")]
    [SerializeField] private float raiseAnimSpeed = 1.0f;           // 상승 애니메이션 속도
    [SerializeField] private float castAnimSpeed = 1.0f;            // 근접 공격 캐스팅 애니메이션 속도
    [SerializeField] private float dashDamage = 20.0f;
    [SerializeField] private float wingDamage = 100.0f;
    [SerializeField] private float soulSphereCastTime = 1.4f;

    [Header("Dash")]
    [SerializeField] private AnimationClip raiseCilp;
    [SerializeField] private float forwardDistanceOffset = 4.0f;    // 대시할 때 타겟 위치보다 더 대시할 거리 (0일 경우 타겟 위치까지만 대시)
    [SerializeField] private float arrivalDistance = 1.0f;          // 도착으로 간주할 거리 (0일 경우 도착 지점일 때 정지)
    [SerializeField] private float dashSpeed = 30.0f;

    [Header("Wing Swipe")]
    [SerializeField] private List<AnimationClip> wingAttackclips = new();

    [Header("Soul Sphere")]
    [SerializeField] SkinnedMeshRenderer smr;                       // 머터리얼을 교체할 렌더러
    [SerializeField] private Material originMaterial;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private GameObject soulSpherePrefab;
    [SerializeField] private int sphereCount = 5;
    [SerializeField] private float spawnRadius = 10.0f;

    [Header("Soul Orb")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private List<Vector3> bulletSpawnOffset = new();

    [Header("Lockdown")]
    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private Vector3 warpPosition;
    [SerializeField] private GameObject chargeEffectPrefab;
    [SerializeField] private Vector3 chargeEffectRotation;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Vector3 effectSpawnOffset;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float monsterSpawnInterval = 1.0f;
    [SerializeField] private Vector2 spawnTargetPositionX;
    [SerializeField] private Vector2 spawnTargetPositionZ;
    [SerializeField] private float spawnTargetPositionY;
    [SerializeField] private List<GameObject> monsterPrefabs = new();
    [SerializeField] private GameObject safeZonePrefab;
    private GameObject _chargeEffect;
    private GameObject _safeZone;
    private List<IDamagable> _targets = new();

    [Header("임시 값")]
    [SerializeField] private bool _isActivate = false;
    [SerializeField] Blackboard blackboard;
    private NavMeshAgent _agent;
    private Animator _animator;
    private PlayerController _target;

    #region Blackboard Version
    public IEnumerator AmonLockdown(Blackboard data)
    {
        // 1. 보스 특정 위치로 이동
        data.NavMeshAgent.Warp(warpPosition);

        // 2. 기 모으기 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetBool("isCharging", true);

        // 3. 기 모으기 이펙트 생성 (본체 위)
        _chargeEffect = Utils.Instantiate(chargeEffectPrefab, data.Agent.transform.position + effectSpawnOffset, Quaternion.Euler(chargeEffectRotation), data.Agent.transform);

        float x = UnityEngine.Random.Range(spawnTargetPositionX.x, spawnTargetPositionX.y);
        float z = UnityEngine.Random.Range(spawnTargetPositionZ.x, spawnTargetPositionZ.y);
        Vector3 spawnPos = new Vector3(x, spawnTargetPositionY + 10.0f, z);
        _safeZone = Utils.Instantiate(safeZonePrefab, spawnPos, Quaternion.identity);

        float elapsed = 0f;
        float spawnTimer = 0f;
        // 4. 캐스팅 시간 동안 몬스터 지속 생성 및 대기
        // Cast Time
        while (elapsed < 10.0f)
        {
            elapsed += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= monsterSpawnInterval)
            {
                spawnTimer = 0f;

                // 필드 내 랜덤 위치 계산
                x = Random.Range(spawnTargetPositionX.x, spawnTargetPositionX.y);
                z = Random.Range(spawnTargetPositionZ.x, spawnTargetPositionZ.y);
                spawnPos = new Vector3(x, spawnTargetPositionY, z); // 높이는 0 고정, 필요시 수정

                int rand = UnityEngine.Random.Range(0, monsterPrefabs.Count);
                GameObject go = Utils.Instantiate(monsterPrefabs[rand], spawnPos, Quaternion.identity);
                IDamagable damagable = go.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    _targets.Add(damagable);
                }
            }

            yield return null;
        }

        // 5.기 모으기 이펙트 제거 및 폭발 이펙트 생성
        if (_chargeEffect != null)
        {
            Utils.Destroy(_chargeEffect);
        }
        Utils.Destroy(Utils.Instantiate(explosionEffectPrefab, data.Agent.transform.position + effectSpawnOffset, Quaternion.identity), 2.0f);
        Utils.Destroy(_safeZone);

        // 6. 플레이어 및 스폰된 몬스터 전체에게 데미지 처리 (임의 함수 호출 예시)
        data.AnimatorParameterSetter.Animator.SetBool("isCharging", false);
        IDamagable target = Managers.MonsterManager.Instance.Player.GetComponent<IDamagable>();
        if (target != null)
        {
            target.ApplyDamage(500.0f, targetMask);
        }
        PlayerController player = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
        if (player)
        {
            player.SetPlayerState(EPlayerState.Invincibility, false);
        }

        // 기믹 종료 후 몬스터를 치우는 걸로 생각하고 있으나 추후 수정 가능
        for (int i = 0; i < _targets.Count; ++i)
        {
            _targets[i].ApplyDamage(10000.0f, targetMask);
        }

        data.NavMeshAgent.Warp(defaultPosition);
    }

    public IEnumerator ShootSoulOrb(Blackboard data)
    {
        _isActivate = true;

        // 1. 캐스팅 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetBool("isCharging", true);

        Vector3 direction = Vector3.zero;

        float elapsed = 0f;
        float castTime = 2.0f;
        while (elapsed < castTime)
        {
            direction = _target.transform.position - data.Agent.transform.position;
            direction.y = 0; // y축 회전만 적용

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                data.Agent.transform.rotation = Quaternion.Slerp(data.Agent.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            elapsed += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 4. 총알 생성 및 발사
        for (int i = 0; i < bulletSpawnOffset.Count; ++i)
        {
            Vector3 startPosition = data.Agent.transform.position + bulletSpawnOffset[i];
            direction = data.Agent.transform.forward;
            direction.y = 0.0f;
            direction.Normalize();
            GameObject orb = Utils.Instantiate(bulletPrefab, startPosition, Quaternion.LookRotation(direction));
            Bullet bullet = orb.GetComponent<Bullet>();
            if (bullet)
            {
                bullet.Init(data.Agent, _target.transform, startPosition, Vector3.zero, direction, 100.0f);
            }
        }
        data.AnimatorParameterSetter.Animator.SetBool("isCharging", false);

        _isActivate = false;
    }

    public IEnumerator InstantiateSoulSphere(Blackboard data)
    {
        _isActivate = true;

        // 1. 안광 머터리얼로 교체
        Material[] mats = smr.materials;    // 복사본 받기
        mats[3] = glowMaterial;             // 복사본 수정
        smr.materials = mats;               // 다시 원본에 할당

        // 2. 캐스팅 대기
        yield return new WaitForSeconds(soulSphereCastTime);

        // 3. 영혼 구체 N개 생성 (랜덤 위치)
        for (int i = 0; i < sphereCount; ++i)
        {
            Vector3 randomPos = data.Agent.transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            randomPos.y = data.Agent.transform.position.y + 0.5f; // 높이 고정, 필요 시 조절
            Utils.Instantiate(soulSpherePrefab, randomPos, Quaternion.identity);
        }

        // 4. 원래 머터리얼로 복구
        mats[3] = originMaterial;
        smr.materials = mats;

        _isActivate = false;
    }

    public IEnumerator AmonWingAttack(Blackboard data)
    {
        _isActivate = true;
        Debug.Log("[Amon Phase 2] 근접 공격 시작");

        // 0. 플레이어 방향 쳐다보기 (Y축 회전만)
        // Target Point
        Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
        lookDir.y = 0;
        data.Agent.transform.rotation = Quaternion.LookRotation(lookDir);

        // 1. 왼쪽 or 오른쪽 공격 결정 (50% 확률)
        bool isLeftAttack = Random.value < 0.5f;

        // 2. 캐스팅 애니메이션 재생
        if (isLeftAttack)
        {
            data.AnimatorParameterSetter.Animator.SetTrigger("CastLeftTrigger");
        }
        else
        {
            data.AnimatorParameterSetter.Animator.SetTrigger("CastRightTrigger");
        }
        data.AnimatorParameterSetter.Animator.speed = castAnimSpeed;

        // Cast Time
        // Anim Speed
        yield return new WaitForSeconds(wingAttackclips[isLeftAttack ? 0 : 1].length / castAnimSpeed);

        // 3. 공격 애니메이션 재생 및 공격 판정 활성화
        if (isLeftAttack)
        {
            data.AnimatorParameterSetter.Animator.SetTrigger("AttackLeftTrigger");
        }
        else
        {
            data.AnimatorParameterSetter.Animator.SetTrigger("AttackRightTrigger");
        }
        data.AnimatorParameterSetter.Animator.speed = 1.0f;
        meleeCollision.gameObject.SetActive(true);

        // Attack Time
        yield return new WaitForSeconds(wingAttackclips[isLeftAttack ? 2 : 3].length);

        // 4. 공격 판정 비활성화 및 Idle 상태 전환
        meleeCollision.gameObject.SetActive(false);
        data.AnimatorParameterSetter.Animator.SetTrigger("IdleTrigger");

        _isActivate = false;
        Debug.Log("[Amon Phase 2] 근접 공격 종료");
    }

    public IEnumerator AmonRush2(Blackboard data)
    {
        Debug.Log("[Amon Phase 2] 돌진 시작");

        // 1. 플레이어 방향 쳐다보기 (Y축 회전만)
        // Target Point
        Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
        lookDir.y = 0;
        data.Agent.transform.rotation = Quaternion.LookRotation(lookDir);

        // 2. 상승 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetTrigger("RaiseTrigger");
        data.AnimatorParameterSetter.Animator.speed = raiseAnimSpeed;

        // 애니메이션 길이에 맞게 대기 (예: 1초)
        yield return new WaitForSeconds(raiseCilp.length / raiseAnimSpeed);

        // 3. 돌진 애니메이션 재생
        data.AnimatorParameterSetter.Animator.speed = 1.0f;
        data.AnimatorParameterSetter.Animator.SetTrigger("DashTrigger");
        meleeCollision.gameObject.SetActive(true);
        //meleeCollision.Init(dashDamage);

        // 4. NavMeshAgent는 바닥에 고정 상태고, 본과 스킨 메시만 움직이므로
        //   agent는 위치 갱신 안하고, 실제 움직임은 Transform.Translate 활용
        lookDir = _target.transform.position - data.Agent.transform.position;
        lookDir.y = 0;
        data.Agent.transform.rotation = Quaternion.LookRotation(lookDir);

        Vector3 dashStartPos = data.Agent.transform.position;
        Vector3 targetPos = _target.transform.position + data.Agent.transform.forward * 4.0f;
        float distance = Vector3.Distance(dashStartPos, targetPos);

        float elapsed = 0f;
        float dashDuration = distance / dashSpeed; // 돌진 시간 계산
        while (elapsed < dashDuration)
        {
            // 방향 업데이트
            Vector3 dir = (targetPos - data.Agent.transform.position).normalized;
            dir.y = 0;

            data.Agent.transform.position += dir * dashSpeed * Time.deltaTime;

            // 도착 체크
            if (Vector3.Distance(data.Agent.transform.position, targetPos) <= arrivalDistance) break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. Idle 애니메이션으로 전환
        data.AnimatorParameterSetter.Animator.SetTrigger("IdleTrigger");
        meleeCollision.gameObject.SetActive(false);

        Debug.Log("[Amon Phase 2] 돌진 종료");
    }
    #endregion

    #region 테스트 함수
    private void Start()
    {
        _animator = GetComponent<Animator>();
        blackboard = gameObject.GetComponent<Blackboard>();

        _agent = GetComponent<NavMeshAgent>();
        _agent.updatePosition = true;
        _agent.updateRotation = true;

        _target = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_isActivate) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //StartCoroutine(AmonRush2());
            StartCoroutine(AmonRush2(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(AmonWingAttack(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(InstantiateSoulSphere(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(ShootSoulOrb(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            StartCoroutine(AmonLockdown(blackboard));
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = new Vector3(
            (spawnTargetPositionX.x + spawnTargetPositionX.y) / 2f,
            spawnTargetPositionY,
            (spawnTargetPositionZ.x + spawnTargetPositionZ.y) / 2f);

        Vector3 size = new Vector3(
            Mathf.Abs(spawnTargetPositionX.y - spawnTargetPositionX.x),
            spawnTargetPositionY + 0.1f, // 얇은 박스 두께
            Mathf.Abs(spawnTargetPositionZ.y - spawnTargetPositionZ.x));

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 반투명 녹색
        Gizmos.DrawCube(center, size);

        // 박스 외곽선 그리기 (선명하게 보이도록)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }

    public IEnumerator AmonRush2()
    {
        _isActivate = true;
        Debug.Log("[Amon Phase 2] 돌진 시작");

        // 1. 플레이어 방향 쳐다보기 (Y축 회전만)
        Vector3 lookDir = _target.transform.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        // 2. 상승 애니메이션 재생
        _animator.SetTrigger("RaiseTrigger");
        _animator.speed = raiseAnimSpeed;

        // 애니메이션 길이에 맞게 대기 (예: 1초)
        yield return new WaitForSeconds(raiseCilp.length / raiseAnimSpeed);

        // 3. 돌진 애니메이션 재생
        _animator.speed = 1.0f;
        _animator.SetTrigger("DashTrigger");
        //meleeCollision.Init(dashDamage);
        meleeCollision.gameObject.SetActive(true);

        // 4. NavMeshAgent는 바닥에 고정 상태고, 본과 스킨 메시만 움직이므로
        //   agent는 위치 갱신 안하고, 실제 움직임은 Transform.Translate 활용
        lookDir = _target.transform.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        Vector3 dashStartPos = transform.position;
        Vector3 targetPos = _target.transform.position + transform.forward * 4.0f;
        float distance = Vector3.Distance(dashStartPos, targetPos);

        float elapsed = 0f;
        float dashDuration = distance / dashSpeed; // 돌진 시간 계산

        while (elapsed < dashDuration)
        {
            // 방향 업데이트
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0;

            transform.position += dir * dashSpeed * Time.deltaTime;

            // 도착 체크
            if (Vector3.Distance(transform.position, targetPos) <= arrivalDistance) break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 6. Idle 애니메이션으로 전환
        _animator.SetTrigger("IdleTrigger");
        meleeCollision.gameObject.SetActive(false);

        Debug.Log("[Amon Phase 2] 돌진 종료");
        _isActivate = false;
    }
    #endregion
}
