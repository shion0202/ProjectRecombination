using System;
using UnityEngine;

public class AmonBarrier : MonoBehaviour, IDamagable
{
    /// - 캐스팅 시작 시 보호막 활성화 되며 보호막이 활성화 되는 동안 받는 모든 대미지 50% 감소
    /// - 보호막이 활성화 된 상태에서 플레이어가 공격하면 보호막이 일정량 대미지를 흡수함
    /// - 보호막이 흡수할 수 있는 대미지의 양은 몬스터의 최대 체력의 10%이며 보호막이 흡수할 수 있는 대미지의 양이 0이 되면 보호막이 비활성화 되고 캐스팅이 취소됨
    /// - 보호막이 활성화 된 상태에서 플레이어가 공격할 때마다 보호막이 흡수할 수 있는 대미지의 양이 감소
    
    private float _maxHealth;
    private int _currentBarrierHealth;
    private bool _isActive;
    
    private event Action OnBarrierDestroy;
    
    public void ApplyDamage(float inDamage, LayerMask targetMask = default, float unitOfTime = 1, float defenceIgnoreRate = 0)
    {
        if (!_isActive) return;    // 비활성(파괴 예약) 상태에서는 더 이상 대미지를 받지 않음

        _currentBarrierHealth -= Mathf.CeilToInt(inDamage);
        Debug.Log($"보스 HP: {_currentBarrierHealth}");

        if (_currentBarrierHealth <= 0)
        {
            Deactivate();
        }
    }

    // 
    private void Deactivate()
    {
        if (!_isActive) return;

        _isActive = false;    // 추가 피격/중복 파괴를 즉시 차단
        Destroy(gameObject);
    }

    public void Initialize(float maxHealth, Action onDestroy = null)
    {
        _maxHealth = maxHealth * 0.1f; // 보호막이 흡수할 수 있는 대미지의 양은 몬스터의 최대 체력의 10%
        OnBarrierDestroy = onDestroy;
        _currentBarrierHealth = Mathf.CeilToInt(_maxHealth); // 보호막이 흡수할 수 있는 대미지의 양은 몬스터의 최대 체력의 10%
        _isActive = true;
    }
    
    private void OnDestroy()
    {
        OnBarrierDestroy?.Invoke();
    }
}
