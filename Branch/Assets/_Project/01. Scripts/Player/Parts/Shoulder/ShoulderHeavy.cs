using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderHeavy : PartBaseShoulder
{
    [SerializeField] protected GameObject shootPrefab;
    [SerializeField] protected GameObject orbPrefab;
    [SerializeField] protected Transform backPart;
    protected SkinnedMeshRenderer smr;
    private Coroutine _skillCoroutine = null;
    protected Coroutine _morphBlendRoutine = null;

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
        GUIManager.Instance.SetBackSkillIcon(true);
        _owner.FollowCamera.SetCameraRotatable(false);
        _owner.SetMovable(false);
        LookCameraDirection();

        // 일정 시간 대기
        // To-do: 이펙트, 애니메이션 등 발사 준비 연출
        _owner.PlayerAnimator.SetBool("isPlayBackHeavyAnim", true);
        yield return new WaitForSeconds(1.0f);

        SkinnedMeshRenderer smr = backPart.GetComponent<SkinnedMeshRenderer>();
        float min = 0.0f;
        float max = 100.0f;

        float elapsed = min;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;

            float weight = Mathf.Lerp(min, max, elapsed / 0.3f);  // weight는 0~100 범위
            smr.SetBlendShapeWeight(0, weight);
            yield return null;
        }

        // 보정: 정확히 1(max)로 설정
        smr.SetBlendShapeWeight(0, max);

        // 오브 생성 및 발사
        Destroy(Instantiate(shootPrefab, transform.position + _owner.transform.forward + Vector3.up, Quaternion.Euler(_owner.transform.rotation.eulerAngles + new Vector3(0.0f, 180.0f, 0.0f))), 2.0f);
        GameObject orb = Instantiate(orbPrefab, transform.position + _owner.transform.forward + Vector3.up, Quaternion.identity);
        Bullet orbComp = orb.GetComponent<Bullet>();
        if (orbComp != null)
        {
            orbComp.Init(_owner.gameObject, null, transform.position + _owner.transform.forward * 1.0f + Vector3.up, Vector3.zero, _owner.transform.forward, 50.0f);
        }

        _owner.PlayerAnimator.SetBool("isPlayBackHeavyAnim", false);
        _owner.FollowCamera.SetCameraRotatable(true);
        _owner.SetMovable(true);

        if (_morphBlendRoutine != null)
        {
            StopCoroutine(_morphBlendRoutine);
        }
        _morphBlendRoutine = StartCoroutine(CoMorphBlend());

        float time = 5.0f;
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

    protected IEnumerator CoMorphBlend(float time = 0.5f)
    {
        SkinnedMeshRenderer smr = backPart.GetComponent<SkinnedMeshRenderer>();
        float min = 100.0f;
        float max = 0.0f;

        float elapsed = min;
        while (elapsed < time)
        {
            elapsed -= Time.deltaTime;
            float weight = Mathf.Lerp(min, max, elapsed / time);
            smr.SetBlendShapeWeight(0, weight);
            yield return null;
        }

        smr.SetBlendShapeWeight(0, max);
        _morphBlendRoutine = null;
    }
}
