using UnityEngine;

namespace Monster
{
    public class MonsterBase : MonoBehaviour
    {
        public void TakeDamage(int damage)
        {
            Debug.Log(gameObject.name + " damaged " + damage);
        }
    }
}