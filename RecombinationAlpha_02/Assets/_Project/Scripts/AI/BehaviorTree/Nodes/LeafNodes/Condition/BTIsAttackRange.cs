using Managers;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTIsInAttackRange", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Condition/BTIsInAttackRange")]
    public class BTIsAttackRange : BTCondition
    {
        public int skillId = 4001; // 스킬 ID, 예시로 1번 스킬 사용
        protected override bool CheckCondition(NodeContext context)
        {
            var rowData = DataManager.Instance.GetRowDataByIndex("MonsterSkill", skillId);
            
            if (rowData == null)
            {
                Debug.LogError($"Skill data not found for skill ID: {skillId}");
                return false;
            }
            
            Debug.Log(rowData.ToString());
            
            var skillRange = rowData.GetStat(EStatType.Range);
            if (skillRange <= 0)
            {
                Debug.LogError($"Invalid skill range: {skillRange} for skill ID: {skillId}");
                return false;
            }
            
            var target = context.Blackboard.Target;
            if (target == null)
            {
                Debug.LogError("Target is null, cannot check attack range.");
                return false;
            }
            
            var agentPosition = context.Blackboard.Agent.transform.position;
            // Calculate the distance to the target
            var distanceToTarget = Vector3.Distance(agentPosition, target.transform.position);
            // Check if the target is within the skill range
            if (distanceToTarget <= skillRange)
            {
                Debug.Log($"Target is within attack range: {distanceToTarget} <= {skillRange}");
                return true;
            }
            else
            {
                Debug.Log($"Target is out of attack range: {distanceToTarget} > {skillRange}");
                return false;
            }
        }
    }
}