using Monster.AI.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Excutioner : MonoBehaviour
{
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

    [Header("임시 값")]
    [SerializeField] private bool _isActivate = false;
    [SerializeField] Blackboard blackboard;
    private NavMeshAgent _agent;
    private Animator _animator;
    private PlayerController _target;

    private IEnumerator ExecutionCriticalOneShoot(Blackboard data)
    {
        // 1. 팔 들기(캐스팅) 애니메이션 재생
        //data.AnimatorParameterSetter.Animator.SetTrigger("CastReady");

        float elapsed = 0f;
        float castingTime = 3.0f;
        while (elapsed < castingTime)
        {
            // 캐스팅 중에도 플레이어 방향으로 회전 가능하게 하려면 이 부분에서 회전
            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion now = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);
                float slerpSpeed = 5.0f;
                data.Agent.transform.rotation = Quaternion.Slerp(now, target, Time.deltaTime * slerpSpeed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 팔 내리기(공격) 애니메이션 재생
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
        _attackRangeInstance = Utils.Instantiate(attackRangePrefab, data.Agent.transform);
        _attackRangeInstance.transform.localPosition = attackRangeOffset;
        _attackRangeInstance.transform.localRotation = Quaternion.identity;

        // 캐스팅 시간 동안 유지
        float elapsed = 0f;
        float slerpSpeed = 2.0f;
        float castingTime = 3.0f;
        while (elapsed < castingTime)
        {
            // 타겟까지의 방향 계산 (Y축만 회전: 수평 회전)
            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion current = data.Agent.transform.rotation;
                Quaternion target = Quaternion.LookRotation(lookDir);

                // Slerp로 부드럽게 회전
                data.Agent.transform.rotation = Quaternion.Slerp(current, target, Time.deltaTime * slerpSpeed);
            }

            // 부채꼴 공격 표시 오브젝트도 agent 전방을 따라간다면 여기서 같이 회전
            //_attackRangeInstance.transform.rotation = Quaternion.LookRotation(data.Agent.transform.forward);

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
    }
    #endregion
}
