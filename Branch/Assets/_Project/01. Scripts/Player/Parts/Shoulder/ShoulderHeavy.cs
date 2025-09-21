using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderHeavy : PartBaseShoulder
{
    [SerializeField] protected GameObject shootPrefab;
    [SerializeField] protected GameObject orbPrefab;
    private Coroutine _skillCoroutine = null;

    public override void UseAbility()
    {
        ShootOrb();
    }

    private void ShootOrb()
    {
        if (_skillCoroutine != null) return;
        _skillCoroutine = StartCoroutine(CoShootOrb());
    }

    protected void LookCameraDirection()
    {
        Camera cam = Camera.main;
        Vector3 lookDirection = cam.transform.forward;
        lookDirection.y = 0; // 수평 방향으로만 회전
        if (lookDirection != Vector3.zero)
            _owner.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    protected IEnumerator CoShootOrb()
    {
        _owner.FollowCamera.SetCameraRotatable(false);
        _owner.SetMovable(false);
        LookCameraDirection();

        // 일정 시간 대기
        // To-do: 이펙트, 애니메이션 등 발사 준비 연출
        yield return new WaitForSeconds(1.0f);

        // 오브 생성 및 발사
        Destroy(Instantiate(shootPrefab, transform.position + _owner.transform.forward + Vector3.up, Quaternion.Euler(_owner.transform.rotation.eulerAngles + new Vector3(0.0f, 180.0f, 0.0f))), 2.0f);
        GameObject orb = Instantiate(orbPrefab, transform.position + _owner.transform.forward + Vector3.up, Quaternion.identity);
        Bullet orbComp = orb.GetComponent<Bullet>();
        if (orbComp != null)
        {
            orbComp.Init(_owner.gameObject, null, transform.position + _owner.transform.forward * 1.0f + Vector3.up, Vector3.zero, _owner.transform.forward, 50.0f);
        }

        _owner.FollowCamera.SetCameraRotatable(true);
        _owner.SetMovable(true);

        yield return new WaitForSeconds(5.0f); // 쿨타임

        Debug.Log("쿨타임 종료");
        _skillCoroutine = null;
    }
}
