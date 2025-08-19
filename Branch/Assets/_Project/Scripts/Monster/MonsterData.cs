using System;

namespace Monster
{
    [Serializable]
    public class MovementSettings
    {
        public float walkSpeed = 2f;
        public float runSpeed = 5f;
    }

    [Serializable]
    public class CombatSettings
    {
        public float lookAtRange = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
        public int maxHealth = 100;
        public int damage = 10;
    }
}