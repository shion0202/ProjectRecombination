using Monster.AI.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AmonPhase1 : MonoBehaviour
{
    [SerializeField] private GameObject meleeCollisionPrefab;       // 근접 공격 범위를 판단할 프리팹 오브젝트
    [SerializeField] private Vector3 collisionScale;                // 근접 공격 범위
    [SerializeField] private Vector3 collisionOffset;               // 근접 공격 위치
    public float rushSpeed;
    private GameObject meleeCollisionObject;

    [Header("임시 값")]
    [SerializeField] private bool _isActivate = false;
    [SerializeField] Blackboard blackboard;
    private NavMeshAgent _agent;
    private Animator _animator;
    private PlayerController _target;

    #region 테스트 함수
    private void Start()
    {
        _animator = GetComponent<Animator>();
        blackboard = gameObject.GetComponent<Blackboard>();

        _agent = GetComponent<NavMeshAgent>();
        _target = Managers.MonsterManager.Instance.Player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_isActivate) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(Rush(blackboard));
        }
    }

    private IEnumerator Rush(Blackboard data)
    {
        Debug.Log("[Amon Phase 1] 돌진 준비");

        // 러시 준비 애니메이션 재생
        data.AnimatorParameterSetter.Animator.SetTrigger("RushReady");

        // 지정된 캐스팅 시간 동안 계속 회전
        float castingTime = 1.0f;
        float elapsed = 0f;
        while (elapsed < castingTime)
        {
            Vector3 lookDir = _target.transform.position - data.Agent.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                lookDir.Normalize();
                data.Agent.transform.rotation = Quaternion.LookRotation(lookDir);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[Amon Phase 1] 돌진 시작");

        // 돌진 방향 설정
        Vector3 directionToTarget = (_target.transform.position - data.Agent.transform.position).normalized;
        data.Agent.transform.LookAt(_target.transform);
        data.AnimatorParameterSetter.Animator.SetTrigger("Rush");

        // 피해를 입힐 근접 공격 범위 오브젝트 생성
        meleeCollisionObject = Utils.Instantiate(meleeCollisionPrefab, data.Agent.transform);
        AmonMeleeCollision meleeCollision = meleeCollisionObject.GetComponent<AmonMeleeCollision>();
        if (meleeCollision)
        {
            // Damage
            meleeCollision.Init(100.0f, collisionScale, collisionOffset);
        }

        float rushDuration = 2.0f; // 돌진 지속 시간
        elapsed = 0f;
        // Sphere 반경 (몬스터 크기에 맞게 조절)
        float sphereRadius = 0.5f;

        while (elapsed < rushDuration)
        {
            // SphereCast로 돌진 경로상의 장애물 충돌 체크
            if (Physics.SphereCast(data.Agent.transform.position, sphereRadius, directionToTarget, out RaycastHit hit, rushSpeed * Time.deltaTime))
            {
                Debug.Log("장애물에 부딪혀 돌진 취소: " + hit.collider.name);
                break;
            }

            // 이동
            data.Agent.transform.position += directionToTarget * rushSpeed * Time.deltaTime;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 돌진 종료
        Debug.Log("[Amon Phase 1] 돌진 종료");
        data.AnimatorParameterSetter.Animator.SetTrigger("RushEnd");
        Utils.Destroy(meleeCollisionObject);
    }
    #endregion
}
