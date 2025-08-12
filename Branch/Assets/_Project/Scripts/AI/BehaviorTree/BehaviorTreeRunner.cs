using System.Collections;
using UnityEngine;

namespace AI.BehaviorTree
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeField] private BehaviorTree tree;
        [SerializeField] private MonsterStats monsterStats;
        
        private void Start() => tree?.Init();
        private void Update()
        {
            // StartCoroutine(TickDelay());
            tree?.Tick(monsterStats);
        }
        
        public BehaviorTree Tree => tree;
        public MonsterStats MonsterStats => monsterStats;

        // private IEnumerator TickDelay()
        // {
        //     // yield return new WaitForEndOfFrame();
        //     yield return new WaitForSeconds(0.1f);
        //     tree?.Tick(monsterStats);
        // }
    }
}
