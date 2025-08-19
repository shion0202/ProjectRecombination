using AI.Blackboard;
using System.Collections.Generic;
using AI.Command;
using Managers;
// using Monster.Skill;
using UnityEngine;

namespace AI
{
    public sealed class AIController : MonoBehaviour
    {
        [SerializeField] private Blackboard.Blackboard blackboard;
        
        // AICommandQueue를 구현하여 AI 명령어를 큐에 저장하고 처리
        private Queue<AICommand> _queue = new();    // TODO: 우선순위 큐를 활용해 우선순위에 따라 명령어를 처리할 수 있도록 개선할 수 있음
        
        public Blackboard.Blackboard Blackboard => blackboard;
        
        public Queue<AICommand> CommandQueue => _queue;
        
        public void EnqueueCommand(AICommand command)
        {
            if (command == null) return;
            _queue.Enqueue(command);
        }
        
        public AICommand DequeueCommand()
        {
            if (_queue.Count == 0) return null;
            return _queue.Dequeue();
        }
        
        #region Unity Methods

        private void Awake()
        {
            // 초기화
            if (blackboard == null)
            {
                blackboard = GetComponent<Blackboard.Blackboard>();
            }
            if (blackboard == null)
            {
                Debug.LogError("Blackboard component is missing on the AIController.");
                return;
            }
            blackboard.InitDefaultValues();
            blackboard.Target = MonsterManager.Instance.Player;
        }

        private void OnEnable()
        {
            // 폴리싱을 위해 AIController가 활성화될 때 초기화 작업을 수행
            if (blackboard == null) return;
            blackboard.Clear();
            blackboard.InitDefaultValues();
            blackboard.Target = MonsterManager.Instance.Player;
        }
        
        // Blackboard의 상태를 매 프레임마다 업데이트할 필요가 있을 경우 사용
        private void Update()
        {
            
        }
        
        // LateUpdate는 매 프레임 후에 호출되므로, AI 명령어 큐를 처리하는 데 적합
        private void LateUpdate()
        {
            if (_queue.Count == 0) return;
            
            // 큐에 명령어가 있는 경우, 첫 번째 명령어를 처리
            var command = DequeueCommand();
            // 명령어를 실행
            command?.Execute(blackboard);
        }
        
        // Gizmo를 사용하여 몬스터의 인식 범위를 시각적으로 표시
        // private void OnDrawGizmosSelected()
        private void OnDrawGizmos()
        {
            if (blackboard is null) return;
            blackboard.InitDefaultValues();
            // 몬스터의 위치를 기준으로 인식 범위를 원으로 표시
            // TODO: 인식 범위를 Blackboard에서 가져와서 표시
            if (blackboard.Agent is not null && blackboard.TryGet(new BBKey<float>("DetectionRange"), out var detectionRange) && detectionRange > 0f)
            {
                // Gizmos의 색상을 설정
                Gizmos.color = Color.red;
                // TODO: 
                Gizmos.DrawWireSphere(blackboard.Agent.transform.position, detectionRange);
            }
        
            // Patrol Waypoints를 시각적으로 표시
            if (blackboard.PatrolInfo.wayPoints is { Length: > 0 })
            {
                Gizmos.color = Color.yellow;
                foreach (var t in blackboard.PatrolInfo.wayPoints)
                {
                    Gizmos.DrawSphere(t, 0.1f);
                }
            }
            
            // 몬스터의 배회 범위를 시각적으로 표시
            if (blackboard.WanderInfo.wanderAreaRadius > 0f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(blackboard.WanderInfo.wanderAreaCenter, blackboard.WanderInfo.wanderAreaRadius);
            }
            
            // 추가 정보는 아래에 추가
        }

        #endregion
    }
}