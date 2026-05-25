using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;
using Cinemachine;

public class ShoulderRapid : PartBaseShoulder
{
    [SerializeField] protected GameObject missilePrefab;
    [SerializeField] protected GameObject targetingPrefab; // 타겟팅 표시용 프리팹
    [SerializeField] protected int maxTargetCount = 12;
    [SerializeField] private float particleStopDelay = 0.9f;  // Inspector에서 조절 가능
    [SerializeField] protected float skillDamage = 100.0f;
    private Coroutine _skillCoroutine = null;
    private List<GameObject> targetingInstances = new List<GameObject>();

    [SerializeField] protected float maxYawAngle = 90f; // 좌우 방향 최대 90도씩 = 180도 범위
    [SerializeField] protected float maxPitchAngle = 10f; // 상하 각도 범위 (조절 가능)
    [SerializeField] protected Vector3 launchOffset = Vector3.zero;

    [SerializeField] protected List<CinemachineVirtualCamera> cutsceneCams = new();
    [SerializeField] protected LayerMask obstacleMask;
    protected CinemachineBrain brain;
    protected CinemachineBlendDefinition defaultBlend;
    protected CinemachineImpulseSource source;

    protected override void Awake()
    {
        base.Awake();

        brain = Camera.main.GetComponent<CinemachineBrain>();
        defaultBlend = brain.m_DefaultBlend;
        source = gameObject.GetComponent<CinemachineImpulseSource>();
    }

    protected void OnEnable()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        for (int i = 0; i < targetingInstances.Count; ++i)
        {
            Utils.Destroy(targetingInstances[i]);
        }
        targetingInstances.Clear();

        brain.m_DefaultBlend = defaultBlend;
        _owner.FollowCamera.SetCameraRotatable(true);
        _owner.SetMovable(true);
        _owner.PlayerAnimator.SetBool("isPlayShoulderAnim", false);
        _owner.SetPlayerState(EPlayerState.Skilling, false);

        for (int i = 0; i < cutsceneCams.Count; ++i)
        {
            cutsceneCams[i].m_Priority = 10;
        }

        if (_currentCooldown <= 0.0f)
        {
            StartCoroutine(SetBackSkillIcon());
        }
    }

    protected void OnDisable()
    {
        GUIManager.Instance.GameUIController.SetBackSkillIcon(false);

        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        for (int i = 0; i < targetingInstances.Count; ++i)
        {
            Utils.Destroy(targetingInstances[i]);
        }
        targetingInstances.Clear();

        brain.m_DefaultBlend = defaultBlend;
        _owner.FollowCamera.SetCameraRotatable(true);
        _owner.SetMovable(true);
        _owner.PlayerAnimator.SetBool("isPlayShoulderAnim", false);
        _owner.SetPlayerState(EPlayerState.Skilling, false);

        for (int i = 0; i < cutsceneCams.Count; ++i)
        {
            cutsceneCams[i].m_Priority = 10;
        }

        if (Managers.GUIManager.IsAliveInstance())
        {
            GUIManager.Instance.GameUIController.SetBackSkillIcon(false);
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(0.0f);
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(false);
        }
    }

    public override void UseAbility()
    {
        if (_cooldownRoutine != null) return;
        LaunchTargetMissiles();
    }

    public override void FinishActionForced()
    {
        base.FinishActionForced();

        GUIManager.Instance.GameUIController.SetBackSkillIcon(false);

        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        for (int i = 0; i < targetingInstances.Count; ++i)
        {
            Utils.Destroy(targetingInstances[i]);
        }
        targetingInstances.Clear();

        brain.m_DefaultBlend = defaultBlend;
        _owner.FollowCamera.SetCameraRotatable(true);
        _owner.SetMovable(true);
        _owner.PlayerAnimator.SetBool("isPlayShoulderAnim", false);
        _owner.SetPlayerState(EPlayerState.Skilling, false);

        for (int i = 0; i < cutsceneCams.Count; ++i)
        {
            cutsceneCams[i].m_Priority = 10;
        }

        if (Managers.GUIManager.IsAliveInstance())
        {
            GUIManager.Instance.GameUIController.SetBackSkillIcon(false);
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(0.0f);
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(false);
        }
    }

    public override IEnumerator CoStartCooldown()
    {
        yield return null;
        yield return null;

        GUIManager.Instance.GameUIController.SetBackSkillIcon(true);
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(true);
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(_currentCooldown);

        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            _currentCooldown -= 0.1f;
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(_currentCooldown);
            if (_currentCooldown <= 0.0f)
            {
                _currentCooldown = 0.0f;
                break;
            }
        }

        GUIManager.Instance.GameUIController.SetBackSkillIcon(false);
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(false);
        _cooldownRoutine = null;
    }

    private void LaunchTargetMissiles()
    {
        // 스킬 시전 하는 동안 카메라, 플레이어 이동 불가
        // 캐릭터가 카메라 방향을 바라봄
        // 화면 범위 내의 적을 조준(타겟팅), 최대 수치까지 타겟팅 가능
        // 타겟팅된 적에게 미사일 발사, 최대 수치가 아닐 경우 남은 미사일은 타겟팅된 적들에게 균등 분배
        if (_skillCoroutine != null) return;

        // 1. 스킬 시전 중 플레이어와 카메라 조작 불가
        _owner.FollowCamera.SetCameraRotatable(false);
        _owner.SetMovable(false);
        _owner.SetPlayerState(EPlayerState.Skilling, true);

        // 2. 플레이어가 카메라 방향 바라봄
        LookCameraDirection();

        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.3f);
        cutsceneCams[0].m_Priority = 100;
        GUIManager.Instance.GameUIController.SetBackSkillIcon(true);

        _skillCoroutine = StartCoroutine(CoLaunchTargetMissiles());
    }

    protected void LookCameraDirection()
    {
        Camera cam = Camera.main;
        Vector3 lookDirection = cam.transform.forward;
        lookDirection.y = 0; // 수평 방향으로만 회전
        if (lookDirection != Vector3.zero)
            _owner.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    // 컷씬 카메라 시점 기준 화면 및 사거리 내에 존재하는 유효한 타겟들을 검출합니다.
    List<TargetPoint> FindValidTargets(LayerMask obstacleMask, float maxRange)
    {
        Camera cam = Camera.main;
        List<TargetPoint> result = new List<TargetPoint>();
        TargetPoint[] allTargets = GameObject.FindObjectsOfType<TargetPoint>();

        // [구조 수정] 카메라 관련 투과 보정이 완벽히 처리되어 있으므로, 
        // 유저가 화면으로 보는 시각적 가시성과 판정을 일치시키기 위해 레이캐스트 시작점을 카메라 위치로 설정합니다.
        Vector3 rayStartPoint = cam.transform.position;

        foreach (var target in allTargets)
        {
            if (target == null) continue;
            GameObject obj = target.gameObject;

            // 1. 사거리 검사 (플레이어와 적 사이의 실제 거리 기준)
            float distanceToPlayer = Vector3.Distance(_owner.transform.position, obj.transform.position);
            if (distanceToPlayer > maxRange) continue;

            // 2. 전환된 카메라의 뷰포트 영역 검사
            Vector3 viewportPos = cam.WorldToViewportPoint(obj.transform.position);

            // [정밀도 개선] 코루틴 타이밍으로 인한 뷰포트 행렬 연산의 미세한 튐 현상 및 
            // 연출 카메라 구도 가장자리에 적이 걸려 연산에서 탈락하는 문제를 방지하기 위해 오차 마진(약 8%)을 부여합니다.
            bool isVisible = viewportPos.z > 0 &&
                             viewportPos.x >= -0.08f && viewportPos.x <= 1.08f &&
                             viewportPos.y >= -0.08f && viewportPos.y <= 1.08f;

            if (!isVisible) continue;

            // 3. 시야 차단 엄폐물(ObstacleMask) 검사
            // 카메라에서 적의 중심점을 향하는 방사형 벡터와 거리를 구합니다.
            Vector3 directionToTarget = obj.transform.position - rayStartPoint;
            float rayDistance = directionToTarget.magnitude;

            // ObstacleMask(벽, 장애물 등) 레이어만 감지하도록 레이캐스팅을 수행합니다.
            // 레이의 길이를 카메라에서 적까지의 직선 거리(rayDistance)로 제한했기 때문에, 
            // 충돌이 발생했다는 것은 적 본인이 아니라 '적을 가로막고 있는 장애물'이 존재한다는 의미가 됩니다.
            if (Physics.Raycast(rayStartPoint, directionToTarget.normalized, out RaycastHit hit, rayDistance, obstacleMask))
            {
                // 충돌이 발생한 오브젝트가 ObstacleMask에 속한 구조물이므로, 시야가 가려진 완전 엄폐 상태로 판단하여 제외합니다.
                continue;
            }

            // 모든 조건을 만족한 유효한 적만 최종 타겟으로 등록
            result.Add(target);
        }

        return result;
    }

    protected Vector3 GetRandomDirection(Vector3 forward)
    {
        // 정면 벡터를 기반으로 정렬된 회전값을 만듭니다.
        Quaternion lookRot = Quaternion.LookRotation(forward);

        // 원추형 분산을 위해 반지름과 각도를 이용한 무작위 오프셋을 구합니다.
        float randomRadiusY = Random.Range(-maxYawAngle, maxYawAngle);
        float randomRadiusX = Random.Range(-maxPitchAngle, maxPitchAngle);

        // 롤(Roll) 각도는 앞방향 자체의 꼬임이므로 무작위로 전방위(0~360도)를 바라볼 수 있게 배치하여 분산을 입체화합니다.
        float randomRoll = Random.Range(0f, 360f);

        Quaternion spreadRot = Quaternion.Euler(randomRadiusX, randomRadiusY, randomRoll);

        // 원본 바라보는 방향에 무작위 분산 회전값을 결합하여 최종 방향을 도출합니다.
        return lookRot * spreadRot * Vector3.forward;
    }

    private IEnumerator CoLaunchTargetMissiles()
    {
        yield return new WaitForSeconds(0.5f);

        // 3. 화면 내의 적을 감지(카메라 시야각/범위 외 적 제외)
        List<TargetPoint> targets = FindValidTargets(obstacleMask, 40.0f);
        if (targets.Count > maxTargetCount)
        {
            targets = targets.GetRange(0, maxTargetCount);
        }

        // 4. 타겟마다 targetingPrefab 생성(시각적 타겟 표시)
        foreach (var enemy in targets)
        {
            Vector3 targetPoint = enemy.transform.position;

            GameObject targeting = Utils.Instantiate(targetingPrefab, targetPoint, Quaternion.identity, enemy.transform);
            targetingInstances.Add(targeting);

            ParticleSystem ps = targeting.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                StartCoroutine(StopParticleAfterDelay(ps));
            }

            // 타겟팅 후 다음 타겟팅 전까지 잠깐 대기
            yield return new WaitForSeconds(0.2f);
        }

        // 5. 각 타겟에게 유도 미사일 발사
        // Count된 적이 없을 경우 종료
        int targetCount = targets.Count;
        if (targetCount <= 0)
        {
            for (int i = 0; i < targetingInstances.Count; ++i)
            {
                Utils.Destroy(targetingInstances[i]);
            }
            targetingInstances.Clear();

            brain.m_DefaultBlend = defaultBlend;
            _owner.FollowCamera.SetCameraRotatable(true);
            _owner.SetMovable(true);
            _owner.PlayerAnimator.SetBool("isPlayShoulderAnim", false);
            _owner.SetPlayerState(EPlayerState.Skilling, false);

            for (int i = 0; i < cutsceneCams.Count; ++i)
            {
                cutsceneCams[i].m_Priority = 10;
            }

            GUIManager.Instance.GameUIController.SetBackSkillIcon(false);
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(0.0f);
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(false);

            _skillCoroutine = null;
            yield break;
        }

        _owner.PlayerAnimator.SetBool("isPlayShoulderAnim", true);
        yield return new WaitForSeconds(0.4f);

        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.1f);
        cutsceneCams[0].m_Priority = 10;

        yield return new WaitForSeconds(0.3f);

        _owner.FollowCamera.ApplyShake(source);

        // 타겟팅 프리팹 제거 (시각 효과 종료)
        foreach (var inst in targetingInstances)
        {
            Utils.Destroy(inst);
        }

        int missilesPerTarget = maxTargetCount / targetCount; // 기본 분배 수
        int remainder = maxTargetCount % targetCount;         // 나머지 미사일 수

        // [개선 구조] 미사일들이 한 프레임에 겹쳐서 나가지 않도록, 모든 발사 대상을 순차 루프로 통합 전개합니다.
        for (int i = 0; i < targetCount; i++)
        {
            int missilesToFire = missilesPerTarget + (i < remainder ? 1 : 0);
            TargetPoint enemy = targets[i];

            for (int j = 0; j < missilesToFire; j++)
            {
                Vector3 targetPoint = enemy.transform.position;
                Vector3 camShootDirection = (targetPoint - transform.position).normalized;
                Vector3 randomDir = GetRandomDirection(camShootDirection);

                GameObject missile = Utils.Instantiate(missilePrefab, _owner.transform.position + launchOffset, Quaternion.LookRotation(randomDir));
                var missileComp = missile.GetComponent<Missile>();
                if (missileComp != null)
                {
                    missileComp.Parent = transform;
                    missileComp.Init(_owner.gameObject, enemy.transform, transform.position, targetPoint, randomDir, skillDamage);
                }

                // [연출 개선] 미사일 한 발 발사할 때마다 눈에 보이지 않을 만큼 미세한 시간 차(약 0.03초)를 둡니다.
                // 이 처리만으로 미사일이 겹치지 않고 일렬 혹은 연속적인 연사 궤적을 그리며 뿜어져 나가게 됩니다.
                yield return new WaitForSeconds(0.03f);
            }
        }

        // 6. 플레이어와 카메라의 조작 재개
        targetingInstances.Clear();

        brain.m_DefaultBlend = defaultBlend;
        _owner.FollowCamera.SetCameraRotatable(true);
        _owner.SetMovable(true);
        _owner.PlayerAnimator.SetBool("isPlayShoulderAnim", false);
        _owner.SetPlayerState(EPlayerState.Skilling, false);

        for (int i = 0; i < cutsceneCams.Count; ++i)
        {
            cutsceneCams[i].m_Priority = 10;
        }

        _currentCooldown = skillCooldown - _owner.Stats.TotalStats[EStatType.CooldownReduction].value;
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(true);
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(_currentCooldown);
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            _currentCooldown -= 0.1f;
            GUIManager.Instance.GameUIController.SetBackSkillCooldown(_currentCooldown);
            if (_currentCooldown <= 0.0f)
            {
                _currentCooldown = 0.0f;
                break;
            }
        }

        GUIManager.Instance.GameUIController.SetBackSkillIcon(false);
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(0.0f);
        GUIManager.Instance.GameUIController.SetBackSkillCooldown(false);
        Debug.Log("쿨타임 종료");
        _skillCoroutine = null;
    }

    private IEnumerator StopParticleAfterDelay(ParticleSystem ps)
    {
        yield return new WaitForSeconds(particleStopDelay);

        if (ps != null)
        {
            ps.Pause();
        }
    }

    private IEnumerator SetBackSkillIcon()
    {
        yield return null;
        GUIManager.Instance.GameUIController.SetBackSkillIcon(false);
    }
}
