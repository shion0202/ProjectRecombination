using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class ShoulderLaser : PartBaseShoulder
{
    [SerializeField] protected GameObject beamLineRendererPrefab;
    [SerializeField] protected GameObject beamStartPrefab;
    [SerializeField] protected GameObject beamEndPrefab;
    [SerializeField] protected GameObject bulletPrefab;

    [SerializeField] protected float beamDamage = 300.0f;
    [SerializeField] protected Vector3 beamOffset = Vector3.zero;
    [SerializeField] protected float beamDuration = 2.0f;
    [SerializeField] protected float beamCooldown = 5.0f;
    [SerializeField] protected float beamMaxDistance = 100.0f;
    [SerializeField] protected float beamRadius = 1.0f;
    [SerializeField] protected LayerMask obstacleMask;

    private GameObject beamStart;
    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;
    private bool _isShooting = false;
    private Coroutine _skillCoroutine = null;
    private float _timer = 0.05f;
    private float _currentTimer = 0.0f;

    protected void Update()
    {
        if (!_isShooting) return;

        _currentTimer += Time.deltaTime;
        if (_currentTimer < _timer) return;
        _currentTimer = 0.0f;

        Vector3 origin = transform.position + (_owner.transform.right * beamOffset.x + _owner.transform.up * beamOffset.y + _owner.transform.forward * beamOffset.z);
        RaycastHit[] hits;
        Vector3 targetPoint = GetTargetPoint(origin, out hits);

        ShootBeamInDir(origin, targetPoint);
    }

    public override void UseAbility()
    {
        ShootLaser();
    }

    protected void ShootLaser()
    {
        if (_isShooting || _skillCoroutine != null) return;

        // 캐릭터가 카메라 방향을 바라보고, 특정 위치에서 레이저가 시작하며, N초간 지속되도록 설정
        LookCameraDirection();
        _skillCoroutine = StartCoroutine(CoStopAndCooldown());
    }

    protected void LookCameraDirection()
    {
        Camera cam = Camera.main;
        Vector3 lookDirection = cam.transform.forward;
        lookDirection.y = 0; // 수평 방향으로만 회전
        if (lookDirection != Vector3.zero)
        {
            _owner.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    protected Vector3 GetTargetPoint(Vector3 origin, out RaycastHit[] hits)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 startPoint = _owner.FollowCamera.transform.position + _owner.FollowCamera.transform.forward;
        float maxDistance = beamMaxDistance;
        Vector3 targetPoint = Vector3.zero;

        // 벽 충돌 위치 찾기 (CapsuleCast에서 첫 충돌)
        if (Physics.Raycast(startPoint, ray.direction, out RaycastHit hitInfo, maxDistance, obstacleMask))
        {
            targetPoint = hitInfo.point;
            maxDistance = hitInfo.distance;  // 벽에 닿는 거리로 범위 제한
        }
        Vector3 targetDirection = targetPoint != Vector3.zero ? (targetPoint - origin).normalized : ray.direction;

        // 제한된 범위 내 모든 충돌 정보 수집
        hits = Physics.CapsuleCastAll(origin, origin, beamRadius, targetDirection, maxDistance, targetMask);

        // 적 데미지 처리
        foreach (var hit in hits)
        {
            if (!hit.transform.TryGetComponent<IDamagable>(out var monster))
            {
                monster = hit.transform.GetComponentInParent<IDamagable>();
            }
            if (monster != null)
            {
                monster.ApplyDamage(targetMask, beamDamage * _timer, _timer, 0.0f);
            }

            Utils.Destroy(Utils.Instantiate(bulletPrefab, hit.point, Quaternion.identity), 0.1f);
        }

        //DrawCapsule(origin, targetPoint, beamRadius, Color.yellow, 0.5f);
        return targetPoint;
    }

    void ShootBeamInDir(Vector3 start, Vector3 end)
    {
        line.positionCount = 2;

        line.SetPosition(0, start);
        beamStart.transform.position = start;

        line.SetPosition(1, end);
        beamEnd.transform.position = end;

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        //float distance = Vector3.Distance(start, end);
        //line.sharedMaterial.mainTextureScale = new Vector2(distance / 3.0f, 1);
        //line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * 375.0f, 0);
    }

    void DrawCapsule(Vector3 point1, Vector3 point2, float radius, Color color, float duration = 0)
    {
        int segments = 16; // 원의 세그먼트 수 (원에 가까울수록 정밀)
        float angleStep = 360f / segments;

        // 캡슐 축선 그리기
        Debug.DrawLine(point1, point2, color, duration);

        // 각 끝점에 원 그리기 (XY 평면 기준 예시)
        for (int i = 0; i < segments; i++)
        {
            float angle1 = Mathf.Deg2Rad * i * angleStep;
            float angle2 = Mathf.Deg2Rad * (i + 1) * angleStep;

            Vector3 offset1 = new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
            Vector3 offset2 = new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;

            Debug.DrawLine(point1 + offset1, point1 + offset2, color, duration);
            Debug.DrawLine(point2 + offset1, point2 + offset2, color, duration);
        }
    }

    private IEnumerator CoStopAndCooldown()
    {
        GUIManager.Instance.SetBackSkillIcon(true);

        _owner.PlayerAnimator.SetBool("isPlayBackLaserAnim", true);
        yield return new WaitForSeconds(0.5f);

        _owner.PlayerAnimator.SetBool("isPlayBackShootAnim", true);
        yield return new WaitForSeconds(0.5f);

        _isShooting = true;
        _owner.SetPlayerState(EPlayerState.Skilling, true);
        beamStart = Utils.Instantiate(beamStartPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        beamEnd = Utils.Instantiate(beamEndPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        beam = Utils.Instantiate(beamLineRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        line = beam.GetComponent<LineRenderer>();

        yield return new WaitForSeconds(beamDuration);

        _isShooting = false;
        _owner.SetPlayerState(EPlayerState.Skilling, false);
        Utils.Destroy(beamStart);
        Utils.Destroy(beamEnd);
        Utils.Destroy(beam);

        _owner.PlayerAnimator.SetBool("isPlayBackShootAnim", false);
        _owner.PlayerAnimator.SetBool("isPlayBackLaserAnim", false);

        float time = beamCooldown;
        GUIManager.Instance.SetBackSkillCooldown(true);
        GUIManager.Instance.SetBackSkillCooldown(time);
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            time -= 0.1f;
            GUIManager.Instance.SetBackSkillCooldown(time);
            if (time <= 0.0f)
            {
                break;
            }
        }

        GUIManager.Instance.SetBackSkillIcon(false);
        GUIManager.Instance.SetBackSkillCooldown(false);
        Debug.Log("쿨타임 종료");
        _skillCoroutine = null;
    }
}
