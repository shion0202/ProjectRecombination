using UnityEngine;

public interface IDamagable
{
    public void ApplyDamage(LayerMask targetMask, float inDamage, float defenceIgnoreRate = 0.0f);
}
