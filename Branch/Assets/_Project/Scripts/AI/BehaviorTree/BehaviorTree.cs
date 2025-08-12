using System;
using System.Collections.Generic;
using AI.BehaviorTree.Nodes;
using UnityEditor;
using UnityEngine;

namespace AI.BehaviorTree
{
    [CreateAssetMenu(menuName = "BehaviorTree/Tree")]
    public class BehaviorTree : ScriptableObject
    {
        public BTNode rootNode;

        public void Init()
        {
            if (rootNode == null)
            {
                Debug.LogWarning("[BehaviorTree] Root node is null.");
                return;
            }

            // Validate and fix GUIDs for all nodes
            var allNodes = GetAllNodes();
            foreach (var node in allNodes)
            {
                if (string.IsNullOrEmpty(node.guid))
                {
                    node.guid = Guid.NewGuid().ToString();
                    Debug.LogWarning($"[BehaviorTree] Fixed missing GUID for node: {node.name}");
                }
            }

            rootNode?.OnValidateNode();
        }

        public NodeState Tick(MonsterStats monsterStats)
        {
            var visited = new HashSet<BTNode>();
            return rootNode?.Evaluate(monsterStats, visited) ?? NodeState.Failure;
        }
        
        private List<BTNode> GetAllNodes()
        {
            var nodes = new List<BTNode>();
            CollectNodes(rootNode, nodes);
            return nodes;
        }

        private void CollectNodes(BTNode node, List<BTNode> nodes)
        {
            if (node == null || nodes.Contains(node)) return;
            nodes.Add(node);

            if (node is BTComposite composite)
            {
                foreach (var child in composite.children)
                    CollectNodes(child, nodes);
            }
            else if (node is BTDecorator decorator && decorator.child != null)
            {
                CollectNodes(decorator.child, nodes);
            }
        }

        public void OnEnable()
        {
            // 트리 에셋이 생성될 때 루트 노드가 없으면 자동으로 셀렉터 노드("루트 노드") 생성
            if (rootNode != null) return;
            
            var selector = CreateInstance<BTSelector>();
            selector.name = "Root";
            selector.guid = Guid.NewGuid().ToString();
            rootNode = selector;
            AssetDatabase.AddObjectToAsset(selector, this);
            AssetDatabase.SaveAssets();
            Debug.Log("[BehaviorTree] 루트 노드가 자동 생성되었습니다.");
        }

        // public BTNode GetNodeByGuid(string guid)
        // {
        //     foreach (var node in GetAllNodes())
        //     {
        //         if (node != null && node.guid == guid)
        //             return node;
        //     }
        //     return null;
        // }
        //
        // 트리 에셋에 포함된 모든 BTNode 반환
        public List<BTNode> GetAllAssetNodes()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var nodes = new List<BTNode>();
            foreach (var asset in assets)
            {
                if (asset is BTNode node)
                    nodes.Add(node);
            }
            return nodes;
        }
    }
}
