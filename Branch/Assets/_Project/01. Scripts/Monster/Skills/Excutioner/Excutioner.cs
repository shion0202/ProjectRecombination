using FIMSpace.FProceduralAnimation;
using Monster.AI.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Excutioner : MonoBehaviour
{
    private float slerpSpeed = 1.0f;

    [Header("Whip Strike")]
    [SerializeField] private GameObject attackRangePrefab;      // 부채꼴 공격 범위 시각화 프리팹
    [SerializeField] private Vector3 attackRangeOffset;
    [SerializeField] private GameObject meleeCollisionPrefab;
    [SerializeField] private Vector3 collisionScale;
    [SerializeField] private Vector3 collisionOffset;
    [SerializeField] private AnimationClip attackClip;
    private GameObject _attackRangeInstance;
    private GameObject _meleeCollisionObject;

    [Header("Critical One Shoot")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPosition;
    [SerializeField] private AnimationClip shootClip;

    [Header("Laser")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Vector3 laserOffset;
    private Transform _shootPoint;

    [Header("Leg Strike")]
    [SerializeField] private AnimationClip legClip;
    [SerializeField] private Vector3 legCollisionScale;
    [SerializeField] private Vector3 legCollisionOffset;
    [SerializeField] private LegsAnimator legAnimator;

    [Header("Launch")]
    public Transform missileOrigin;          // 미사일 발사 위치 (등 부위)
    [SerializeField] private GameObject missilePrefab;         // 미사일 프리팹
    public GameObject aoeFieldPrefab;        // 원형 장판 프리팹
    public int aoeFieldCount = 5;             // 생성할 장판 개수
    public float aoeRadius = 3f;              // 장판 반경 (3x3 원형)
    public float aoeDuration = 2f;            // 장판 지속 시간
    public float missileFallSpeed = 10f;     // 미사일 낙하 속도
    private GameObject spawnedMissile;
    private GameObject[] aoeFields;

    [Header("임시 값")]
    [SerializeField] private bool _isActivate = false;
    [SerializeField] Blackboard blackboard;
    private NavMeshAgent _agent;
    private Animator _animator;
    private PlayerController _target;

    private IEnumerator ExecutionerLaunch(Blackboard data)
    {
        // 1. 미사일 생성 및 발사 애니메이션 트리거 (별도 애니메이션 함수 호출 가능)
        spawnedMissile = Utils.Instantiate(missilePrefab, missileOrigin.position, Quaternion.identity);

        // 미사일 위로 발사
        Vector3 targetPosition = missileOrigin.position + Vector3.up * 50.0f;
        float travelTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            spawnedMissile.transform.position = Vector3.Lerp(missileOrigin.position, targetPosition, elapsed / travelTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 플레이어 주변에 장판 N개 원형 배치
        aoeFields = new GameObject[aoeFieldCount];
        for (int i = 0; i < aoeFieldCount; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * aoeRadius;
            Vector3 spawnPos = new Vector3(_target.transform.position.x + randomOffset.x, _target.transform.position.y, _target.transform.position.z + randomOffset.y);
            aoeFields[i] = Instantiate(aoeFieldPrefab, spawnPos, Quaternion.identity);

            // 장판 크기 조절 등 추가 작업 가능
        }

        // 3. 2초간 대기 (장판 유지)
        yield return new WaitForSeconds(aoeDuration);

        // 4. 미사일 낙하 및 데미지 적용
        

        // 장판 오브젝트 정리
        for (int i = 0; i < aoeFieldCount; i++)
        {
            if (aoeFields[i] != null)
            {
                Utils.Destroy(aoeFields[i]);
            }
        }

        if (spawnedMissile != null)
        {
            Utils.Destroy(spawnedMissile);
        }
    }

    private IEnumerator ExecutionerLegStrike(Blackboard data)
    {
        float elapsed = 0f;
        float castingTime = 1.0f;
        while (elapsed < castingTime)
        {
            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion now = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);
                data.Agent.transform.rotation = Quaternion.Slerp(now, target, Time.deltaTime * slerpSpeed * 3.0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _meleeCollisionObject = Utils.Instantiate(meleeCollisionPrefab, data.Agent.transform);
        _meleeCollisionObject.transform.localPosition = Vector3.zero;
        _meleeCollisionObject.transform.localRotation = Quaternion.identity;

        var meleeComp = _meleeCollisionObject.GetComponent<AmonMeleeCollision>();
        if (meleeComp != null)
        {
            meleeComp.Init(100.0f, legCollisionScale, legCollisionOffset);
        }

        legAnimator.MainGlueBlend = 0.0f;
        data.AnimatorParameterSetter.Animator.SetBool("isLegStrike", true);

        // 공격 판정 오브젝트 지속 시간 대기 후 파괴
        // To-do: 애니메이션 추가 후, 애니메이션 재생 + 시간 만큼 유지되도록 수정
        yield return new WaitForSeconds(legClip.length);

        data.AnimatorParameterSetter.Animator.SetBool("isLegStrike", false);
        Utils.Destroy(_meleeCollisionObject);
        legAnimator.MainGlueBlend = 100.0f;
    }

    private IEnumerator ExecutionerLaser(Blackboard data)
    {
        float elapsed = 0f;

        // 1. 캐스팅 중 레이저 본이 느리게 플레이어를 따라감
        float castTime = 2.0f;
        while (elapsed < castTime)
        {
            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion now = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);
                data.Agent.transform.rotation = Quaternion.Slerp(now, target, Time.deltaTime * slerpSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        /*
         * To -do: 현재 TargetPos를 정확히 추적하고 있어 패턴을 피할 수 없는 상태
         * 이전에 플레이어가 이동할 경우 서서히 플레이어를 추적하는 Auto target point에 대한 얘기가 나왔었는데,
         * Monster Manager 등에서 해당 Transform 정보를 관리하도록 하고 몬스터가 조준할 Target Point를 해당 오브젝트로 설정하는 건 어떤지?
         * 추후 얘기해보아야 하는데 까먹을 수 있어서 일단 주석으로도 작성해두겠음
         */

        // 2. 캐스팅 완료 후, 타겟 방향으로 레이저 발사
        GameObject shootObject = Utils.Instantiate(new GameObject());
        _shootPoint = shootObject.transform;
        _shootPoint.transform.SetParent(data.Agent.transform);
        _shootPoint.transform.localPosition = laserOffset;
        _shootPoint.transform.localRotation = Quaternion.identity;

        GameObject laser = Utils.Instantiate(laserPrefab, _shootPoint);
        laser.transform.localPosition = Vector3.zero;
        laser.transform.localRotation = Quaternion.identity;

        data.AnimatorParameterSetter.Animator.SetBool("isLaser", true);

        float duration = 4.0f;
        while (elapsed < duration)
        {
            laser.transform.rotation = Quaternion.LookRotation(_target.transform.position - _shootPoint.position);

            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion now = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);
                data.Agent.transform.rotation = Quaternion.Slerp(now, target, Time.deltaTime * slerpSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        data.AnimatorParameterSetter.Animator.SetBool("isLaser", false);
        Utils.Destroy(laser);
        Utils.Destroy(shootObject);
        _shootPoint = null;
    }

    private IEnumerator ExecutionCriticalOneShoot(Blackboard data)
    {
        // To-do: 팔 IK 적용 필요 (Animation Rigging Package의 Multi-Aim Constraint 컴포넌트)
        // 1. 팔 들기(캐스팅) 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetTrigger("shootCastingTrigger");

        float elapsed = 0f;
        float castingTime = 5.0f;
        while (elapsed < castingTime)
        {
            // 캐스팅 중에도 플레이어 방향으로 회전 가능하게 하려면 이 부분에서 회전
            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion now = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);
                data.Agent.transform.rotation = Quaternion.Slerp(now, target, Time.deltaTime * slerpSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 사격 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetBool("isShoot", true);

        // 3. 불렛(원거리 공격체) 생성 및 발사
        Vector3 targetPos = _target.transform.position;
        Vector3 shootDir = (targetPos - shootPosition.position).normalized;

        GameObject go = Utils.Instantiate(bulletPrefab, shootPosition.position, Quaternion.LookRotation(shootDir));
        Bullet bullet = go.GetComponent<Bullet>();
        if (bullet)
        {
            bullet.Init(data.Agent, _target.transform, shootPosition.position, _target.transform.position, _target.transform.position - shootPosition.position, 300.0f);
        }

        yield return new WaitForSeconds(shootClip.length);
        data.AnimatorParameterSetter.Animator.SetBool("isShoot", false);
    }

    private IEnumerator ExcutionWhipStrike(Blackboard data)
    {
        // 1. 부채꼴 공격 범위 표시 (예: 몬스터 앞에 표시)
        Vector3 targetDir = _target.transform.position - data.Agent.transform.position;
        _attackRangeInstance = Utils.Instantiate(attackRangePrefab, data.Agent.transform.position, Quaternion.identity);
        _attackRangeInstance.transform.localPosition += targetDir.normalized * attackRangeOffset.z;
        _attackRangeInstance.transform.localRotation = Quaternion.LookRotation(targetDir);

        // 캐스팅 시간 동안 유지
        float elapsed = 0f;
        float castingTime = 3.0f;
        Vector3 targetPos = _target.transform.position;
        while (elapsed < castingTime)
        {
            // 타겟까지의 방향 계산 (Y축만 회전: 수평 회전)
            Vector3 lookDir = targetPos - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion current = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);

                // Slerp로 부드럽게 회전
                data.Agent.transform.rotation = Quaternion.Slerp(current, target, Time.deltaTime * slerpSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 캐스팅 끝, 표시 오브젝트 제거
        Utils.Destroy(_attackRangeInstance);

        // 3. 공격 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetBool("isChainsword", true);

        // 4. 공격 판정 오브젝트 생성 후 초기화
        _meleeCollisionObject = Utils.Instantiate(meleeCollisionPrefab, data.Agent.transform);
        _attackRangeInstance.transform.localPosition = Vector3.zero;
        _attackRangeInstance.transform.localRotation = Quaternion.identity;

        var meleeComp = _meleeCollisionObject.GetComponent<AmonMeleeCollision>();
        if (meleeComp != null)
        {
            meleeComp.Init(100.0f, collisionScale, collisionOffset);
        }

        // 공격 판정 오브젝트 지속 시간 대기 후 파괴
        // To-do: 애니메이션 추가 후, 애니메이션 재생 + 시간 만큼 유지되도록 수정
        yield return new WaitForSeconds(attackClip.length);

        data.AnimatorParameterSetter.Animator.SetBool("isChainsword", false);
        Utils.Destroy(_meleeCollisionObject);
    }

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
            StartCoroutine(ExcutionWhipStrike(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(ExecutionCriticalOneShoot(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(ExecutionerLaser(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(ExecutionerLegStrike(blackboard));
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            StartCoroutine(ExecutionerLaunch(blackboard));
        }
    }
    #endregion
}
