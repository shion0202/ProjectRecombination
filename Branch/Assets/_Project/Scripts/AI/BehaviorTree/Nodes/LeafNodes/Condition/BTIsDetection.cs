using Unity.VisualScripting;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Condition/IsDetection")]
    public class BTIsDetection : BTCondition
    {
        protected override bool CheckCondition(MonsterStats monsterStats)
        {
            // Check if the target is within the detection range
            if (monsterStats.Target is null) return false;
            
            var targetPosition = monsterStats.Target.transform.position;
            var agentPosition = monsterStats.Agent.transform.position;
            var detectionRange = monsterStats.DetectionRange;
            
            // Calculate the distance between the agent and the target
            var distance = Vector3.Distance(agentPosition, targetPosition);
            
            // If the distance is less than or equal to the detection range, return true
            return distance <= detectionRange;
        }
    }
}