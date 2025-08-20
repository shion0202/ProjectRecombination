using Managers;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTActionAttack", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Actions/BTActionAttack")]
    public class BTActionAttack : BTAction
    {
        public int skillId = 4001; // 스킬 ID, 예시로 1번 스킬 사용
        public override NodeState Evaluate(NodeContext context, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            var skillData = DataManager.Instance.GetRowDataByIndex("MonsterSkill", skillId);
            if (skillData == null)
            {
                Debug.LogError($"Skill data not found for skill ID: {skillId}");
                return NodeState.Failure;
            }
            Debug.Log("Monster is Using Skill" + skillId);
            // blackboard.State = MonsterState.Attack;
            Debug.Log(skillData.ToString());

            return state = NodeState.Success;
        }
    }
}