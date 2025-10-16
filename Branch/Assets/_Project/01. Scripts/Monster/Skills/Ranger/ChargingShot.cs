using System.Collections;
using UnityEngine;

namespace _Test.Skills.Ranger
{
    /// <summary>
    /// 스킬 이름: 차징 샷
    /// - 캐스팅 시간 동안 움직이지 않고 플레이어를 향해 사격 자세
    /// - 캐스팅 완료 시 플레이어를 향해 여러발의 총알 발사
    /// </summary>
    [CreateAssetMenu(fileName = "ChargingShot", menuName = "MonsterSkills/Ranger/ChargingShot")]
    public class ChargingShot: SkillData
    {
        public int bulletCount = 5; // 발사할 총알 수
        public float bulletInterval = 0.2f; // 총알 발사 간격
        public override IEnumerator Casting(Monster.AI.Blackboard.Blackboard data)
        {
            // 캐스팅 시간 동안 움직이지 않고 플레이어를 향해 사격 자세 취함
            data.AnimatorParameterSetter.Animator.SetTrigger("FireReady");
            yield return base.Casting(data);
        }
        public override IEnumerator Activate(Monster.AI.Blackboard.Blackboard data)
        {
            Debug.Log("차징샷 시전!");
            
            for (int i = 0; i < bulletCount; i++)
            {
                // 플레이어를 향해 총알 발사
                data.AnimatorParameterSetter.Animator.SetTrigger("Fire");
                // 총알 발사 간격 만큼 대기
                yield return new WaitForSeconds(bulletInterval);
            }
            yield return null;
        }
    }
}