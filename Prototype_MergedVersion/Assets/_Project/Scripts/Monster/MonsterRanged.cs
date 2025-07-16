using System;
using System.Collections;
using UnityEngine;

namespace Jaeho.Monster
{
    [Serializable]
    public class RangedStats
    {
        public GameObject projectilePrefab;         // 투사체 프리팹
        public float projectileSpeed = 10f;         // 투사체 속도
        public float dilayBetweenShots = 1f;        // 투사체 발사 간격
        public GameObject projectileSpawnPoint;     // 투사체 발사 위치
        public float spinSpeed = 5f;                // 회전 속도
    }
    
    public class MonsterRanged : MonsterBase
    {
        #region Inspector Variables
        
        [Header("Ranged Monster Settings")]
        [SerializeField] private RangedStats stats;
        
        private bool _isAttacking; // 공격 중인지 여부

        public MonsterRanged(RangedStats stats)
        {
            this.stats = stats;
        }

        #endregion
        
        #region Default Methods

        protected override void Idle()
        {
            // Check if target is set
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for idle state.");
                return;
            }
            // Set idle animation
            {
                // code
            }
            
            // Idle logic for the ranged monster
            if (Vector3.Distance(transform.position, Target.position) > LookAtRange) return;
            State = MonsterState.Chase;
        }
        
        protected override void Chase()
        {
            // Check if target is set
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for chasing.");
                return;
            }
            
            // Set Run animation
            {
                // code
            }
            
            // Chase logic
            Agent.SetDestination(Target.transform.position);
            
            if (Vector3.Distance(transform.position, Target.position) <= AttackRange)
            {
                Agent.isStopped = true;
                State = MonsterState.Attack;
            }
            else if (Vector3.Distance(transform.position, Target.position) > LookAtRange)
            {
                Agent.isStopped = false;
                State = MonsterState.Idle;
            }
        }
        
        protected override void Attack()
        {
            // Check if target is set
            if (Target is null)
            {
                Debug.LogWarning("Target is not set for attacking.");
                return;
            }
            
            // Set Attack animation
            {
                // code
            }
            
            // Attack logic
            // 투사체를 발사하는 방식으로 공격한다.
            // 몬스터는 공격을 위해 타겟을 바라본다.
            // 투사체 발사 후 일정 시간 동안 공격을 멈춘다.
            {
                // y축 회전
                var direction = (Target.position - transform.position).normalized;
                var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * stats.spinSpeed);
                
                StartCoroutine(nameof(FireProjectileWithDelay));
            }
        }
        
        protected override void Dead()
        {
            
        }
        
        #endregion
        
        #region Ranged Specific Methods
        
        // 투사체 관련 변수 체크 메서드
        private bool CheckVariables()
        {
            if (stats.projectilePrefab == null)
            {
                Debug.LogError("Projectile Prefab is not assigned.");
                return false;
            }
            if (stats.projectileSpawnPoint == null)
            {
                Debug.LogError("Projectile Spawn Point is not assigned.");
                return false;
            }
            return true;
        }
        
        // 투사체 발사 메서드
        private void FireProjectile()
        {
            // 투사체 발사 로직
            Debug.Log("Firing projectile at target.");
            if (!CheckVariables())
            {
                Debug.LogError("No projectile object assigned.");
                return;
            }
            
            var projectile = Instantiate(stats.projectilePrefab, stats.projectileSpawnPoint.transform.position, Quaternion.identity);
            var bullet = projectile.GetComponent<Bullet>();
            if (bullet == null)
            {
                Debug.LogError("Projectile prefab does not have a Bullet component.");
                return;
            }
            
            bullet.damage = Damage; // Set the damage, the projectile
            bullet.from = gameObject; // Set the source of the projectile
            
            var direction = CalcProjectileDirection();   
            
            projectile.GetComponent<Rigidbody>().velocity = direction * stats.projectileSpeed; // Set the velocity of the projectile
            Debug.Log($"Projectile fired from {gameObject.name} towards {Target.name} with speed {stats.projectileSpeed}.");
        }
        
        // TODO: 방향 계산 과정에서 보정이 필요하다. (몬스터가 플레이어의 발을 향해 총알을 발사하는 문제가 있다.
        // 현재는 단순히 타겟의 위치를 향해 발사한다.
        private Vector3 CalcProjectileDirection()
        {
            // 투사체가 발사될 방향을 계산하는 메서드
            if (Target == null)
            {
                Debug.LogError("Target is not set for projectile direction calculation.");
                return Vector3.zero;
            }
            
            // 타겟의 자식 오브젝트 중 "TargetPos" 가 있는지 확인
            // 만약 있다면 해당 오브젝트의 위치를 사용하고, 없다면 타겟의 위치를 사용한다.
            var targetPosition = FindChildByName(Target, "TargetPos");
            
            var direction = (targetPosition.position - stats.projectileSpawnPoint.transform.position).normalized;
            return direction;
        }

        private Transform FindChildByName(Transform parent, string _name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == _name) return child;
                var found = FindChildByName(child, _name);
                if (found != null) return found;
            }
            return null;
        }
        
        // 투사체 발사 간격을 관리하는 코루틴
        private IEnumerator FireProjectileWithDelay()
        {
            if (_isAttacking) yield break;
            FireProjectile();
            _isAttacking = true;
            yield return new WaitForSeconds(stats.dilayBetweenShots);
            _isAttacking = false;
        }
        
        #endregion
    }
}