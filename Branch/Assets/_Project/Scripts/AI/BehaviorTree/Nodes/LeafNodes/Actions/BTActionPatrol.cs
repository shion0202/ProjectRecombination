using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(menuName = "BehaviorTree/Action/Patrol")]
    public class BTActionPatrol : BTAction
    {
        private float _startTime;
        private int _currentPatrolIndex;
        private int _postPatrolIndex;
        private bool _isStarted;
        private bool _isPatrolling;
        
        public void OnEnter(MonsterStats monsterStats)
        {
            _startTime = Time.time; // 시작 시간을 기록
            _currentPatrolIndex = Random.Range(0, monsterStats.PatrolPath.Length); // 페트롤 인덱스 초기화
            _isPatrolling = true;
        }
        
        public override NodeState Evaluate(MonsterStats monsterStats, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            if (!_isStarted)
            {
                OnEnter(monsterStats); // 초기화가 안 된 경우 초기화
                _isStarted = true; // 시작 상태 설정
            }
            
            // 패트롤 시간 초과 될 때까지 이동
            if (!_isPatrolling) return state = NodeState.Failure;
            
            if (Time.time - _startTime >= monsterStats.PatrolTimer)
            {
                _isPatrolling = false;
                _isStarted = false;
                monsterStats.NavMeshAgent.isStopped = true; // NavMeshAgent를 멈춤
                monsterStats.NavMeshAgent.ResetPath(); // 현재 경로를 초기화
                return state = NodeState.Failure;
            }
                
            if (monsterStats.NavMeshAgent.remainingDistance <= monsterStats.NavMeshAgent.stoppingDistance)
            {
                _postPatrolIndex = _currentPatrolIndex; // 현재 인덱스를 저장
                // 다음 경로 지점으로 이동
                _currentPatrolIndex = (_currentPatrolIndex + 1) % monsterStats.PatrolPath.Length;
                if (_currentPatrolIndex == _postPatrolIndex)
                    // 다음 경로 지점이 현재 지점과 같으면, 다시 시작
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % monsterStats.PatrolPath.Length;
            }
                
            var targetPosition = monsterStats.PatrolPath[_currentPatrolIndex];
            monsterStats.NavMeshAgent.SetDestination(targetPosition);
                
            monsterStats.State = MonsterState.Patrol;
            return state = NodeState.Running;
        }
    }
}