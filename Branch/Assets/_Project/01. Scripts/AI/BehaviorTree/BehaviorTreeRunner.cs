using System;
using System.Collections;
using AI.BehaviorTree.Nodes;
using AI.Command;
using Blackboard = AI.Blackboard;
using Monster;
using UnityEngine;

namespace AI.BehaviorTree
{
    public readonly struct NodeContext
    {
        public readonly Blackboard.Blackboard Blackboard;
        public readonly Action<AICommand> Enqueue;
        
        public NodeContext(Blackboard.Blackboard blackboard, Action<AICommand> enqueue)
        {
            Blackboard = blackboard;
            Enqueue = enqueue;
        }
    }
    
    public class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeField] private BehaviorTree tree;
        [SerializeField] private AIController aiController;
        private void Start() => tree?.Init();
        private void Update() => tree?.Tick(new NodeContext(aiController.Blackboard, c => aiController.EnqueueCommand(c)));
    }
}
