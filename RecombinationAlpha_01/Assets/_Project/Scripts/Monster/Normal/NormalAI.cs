using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Monster.Normal
{
    public class NormalAI : AI
    {
        #region Unity Methods

        private void Start()
        {   
            // 초기 상태를 Spawn으로 설정
            ChangeState(new Spawn(this));
            
            // 필요한 컴포넌트가 초기화 되었는지 점검한다.
            if (!CheckProperties())
            {
                Debug.LogWarning(Stats.name + " 캐릭터의 상태값이 정상적으로 초기화 되지 않았습니다.");
                return;
            }
                
            // 몬스터 메니저를 통해 타겟을 불러온다.
            SetTarget();

            if (Target is not null) return;
            Debug.LogWarning("전투 대상을 식별할 수 없습니다.");
        }
        
        private void OnDrawGizmosSelected()
        {
            // 현재 Gizmo의 색상을 설정합니다.
            Gizmos.color = Color.blue;

            // 오브젝트의 현재 위치를 Gizmo의 중심으로 사용합니다.
            var center = Body.transform.position;
            
            // 와이어프레임(속이 빈) 구체를 그립니다.
            Gizmos.DrawWireSphere(center, Stats.detectiveRange);
            
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(center, Stats.attackRange);
        }

        #endregion

        #region State Methods

        protected override IEnumerator SpawnCoroutine()
        {
            yield return new WaitForSeconds(0.1f);  // 스폰 상태에서 0.1초 뒤 Idle 상태로 전환
            // ChangeState(new Idle(this));
            IsActing = false;
        }
        
        protected override IEnumerator IdleCoroutine()
        {
            Debug.Log("Is Idle Coroutine");
            ChangeState(new Idle(this));
            
            // idle 애니메이션 재생
            Animator.SetInteger("State", 1);
            
            // var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            //
            // // 애니메이션 길이를 대기 시간으로 설정
            // yield return new WaitForSeconds(stateInfo.length);
            
            IsActing = false;
            yield break;
        }

        protected override IEnumerator PatrolCoroutine()
        {
            ChangeState(new Patrol(this));
            yield return new WaitForSeconds(2.0f);
            
            Debug.Log("Is Patrol...");
            IsActing = false;
        }
        
        protected override IEnumerator ChaseCoroutine()
        {
            if (PreviousCoroutine is not null) StopCoroutine(PreviousCoroutine); // 이전 코루틴 중단
            
            Debug.Log("Is Chase Coroutine");
            // Chase 상태로 전환
            ChangeState(new Chase(this));
            
            // chase 애니메이션 재생
            Animator.SetInteger("State", 2);
            
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            
            // 애니메이션 길이를 대기 시간으로 설정
            yield return new WaitForSeconds(stateInfo.length);
            
            IsActing = false;
        }
        
        protected override IEnumerator AttackCoroutine()
        {
            if (PreviousCoroutine is not null) StopCoroutine(PreviousCoroutine); // 이전 코루틴 중단
            
            Debug.Log("Is Attack Coroutine");
            // Attack 상태로 전환
            ChangeState(new Attack(this));
            
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            
            // 애니메이션 길이를 대기 시간으로 설정
            yield return new WaitForSeconds(stateInfo.length);
            
            Debug.Log("Is Attack...");
            IsActing = false;
        }
        
        protected override IEnumerator EvasionCoroutine()
        {
            yield break;
        }
        protected override IEnumerator HitCoroutine()
        {
            yield break;
        }
        protected override IEnumerator DeathCoroutine()
        {
            yield break;
        }

        #endregion
        
        // 데미지 처리
        public override void TakeDamage(int damage)
        {
            // 현재 체력에서 데미지를 뺀다.
            Stats.currentHealth -= damage;
            
            // 체력이 0 이하가 되면 죽음 상태로 전환
            if (Stats.currentHealth <= 0)
            {
                ChangeState(new Death(this));
            }
            else
            {
                ChangeState(new Hit(this));
            }
        }
    }
}