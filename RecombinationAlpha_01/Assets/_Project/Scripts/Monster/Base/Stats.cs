using System;
using UnityEngine;

namespace Monster
{
    [Serializable]
    public class Stats
    {
        #region Private Properties

        // 기본 정보
        public int index;
        public string name;
        
        // 체력 상태 값
        public int currentHealth;
        public int maxHealth;
        
        // 이동 상태 값
        public int walkSpeed;
        public int runSpeed;
        
        // 전투 스탯
        public int damage;
        public float attackSpeed;
        public int defense;
        
        public float attackRange;       // 더미 공격 범위
        public float detectiveRange;    // 더미 감지 범위

        #endregion

        public bool CheckProperties()
        {
            if (index == 0) return false;
            if (name == null) return false;
            if (maxHealth <= 0) return false;
            if (walkSpeed <= 0) return false;
            if (runSpeed <= 0) return false;
            if (damage < 0) return false;
            if (attackSpeed < 0.0f) return false;
            if (defense < 0) return false;

            {
                if (attackRange < 0.0f) return false;
                if (detectiveRange < 0.0f) return false;
            }
            
            currentHealth = maxHealth;
            
            return true;
        }

        public void Reset()
        {
            currentHealth = maxHealth;
        }
    }
}