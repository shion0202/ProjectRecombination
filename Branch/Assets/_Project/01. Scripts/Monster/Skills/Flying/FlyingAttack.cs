using Monster.AI.Blackboard;
using System.Collections;
using UnityEngine;

namespace _Test.Skills.Flying
{
    [CreateAssetMenu(fileName = "FlyingAttack", menuName = "MonsterSkills/Flying/FlyingAttack")]
    public class FlyingAttack : SkillData
    {
        // [Header("그 외 스킬 정보")] 
        // [SerializeField] private GameObject melleeAttackColliderPrefab;

        // public override IEnumerator Casting(Blackboard data)
        // {
        //     return base.Casting(data);
        // }

        public override IEnumerator Activate(Blackboard data)
        {
            data.Agent.transform.LookAt(data.Target.transform);
            yield return null; // 한 프레임 쉬기
            
            int random = Random.Range(0, 1);
            data.AnimatorParameterSetter.Animator.SetTrigger(random == 0 ? "LAttack" : "RAttack");
        }
    }
}