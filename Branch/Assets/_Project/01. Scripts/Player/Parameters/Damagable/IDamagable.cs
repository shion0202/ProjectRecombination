using UnityEngine;

public interface IDamagable
{
    public void ApplyDamage(LayerMask targetMask, float inDamage, float unitOfTime = 1.0f, float defenceIgnoreRate = 0.0f);
}
