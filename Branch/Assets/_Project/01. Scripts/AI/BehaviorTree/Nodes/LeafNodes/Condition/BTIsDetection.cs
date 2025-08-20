using AI.Blackboard;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTIsDetection", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Condition/BTIsDetection")]
    public class BTIsDetection : BTCondition
    {
        protected override bool CheckCondition(NodeContext context)
        {
            var blackboard = context.Blackboard;
            // Check if the target is within the detection range
            if (blackboard.Target is null) return false;
            
            var targetPosition = blackboard.Target.transform.position;
            var agentPosition = blackboard.Agent.transform.position;

            if (blackboard.TryGet(new BBKey<float>("DetectionRange"), out float detectionRange))
            {
                // Calculate the distance between the agent and the target
                var distance = Vector3.Distance(agentPosition, targetPosition);
            
                // If the distance is less than or equal to the detection range, return true
                return distance <= detectionRange;
            }

            return false;
        }
    }
}