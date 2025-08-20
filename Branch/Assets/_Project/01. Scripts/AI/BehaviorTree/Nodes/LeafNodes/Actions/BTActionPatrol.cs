using System.Collections.Generic;
using AI.Command;
using UnityEngine;

namespace AI.BehaviorTree.Nodes
{
    [CreateAssetMenu(fileName = "BTActionPatrol", menuName = "AI/BehaviorTree/Nodes/LeafNodes/Actions/BTActionPatrol")]
    public class BTActionPatrol : BTAction
    {
        // private float _startTime;
        // private int _currentPatrolIndex;
        // private int _postPatrolIndex;
        // private bool _isStarted;
        // private bool _isPatrolling;
        
        // TODO: 패트롤 기능 다시 구현
        
        // public void OnEnter(Blackboard blackboard)
        // {
        //     _startTime = Time.time; // 시작 시간을 기록
        //     _currentPatrolIndex = Random.Range(0, blackboard.PatrolPath.Length); // 페트롤 인덱스 초기화
        //     _isPatrolling = true;
        // }
        
        public override NodeState Evaluate(NodeContext context, HashSet<BTNode> visited)
        {
            if (CheckCycle(visited))
                return NodeState.Failure;
            
            // if (!_isStarted)
            // {
            //     OnEnter(blackboard); // 초기화가 안 된 경우 초기화
            //     _isStarted = true; // 시작 상태 설정
            // }
            //
            // // 패트롤 시간 초과 될 때까지 이동
            // if (!_isPatrolling) return state = NodeState.Failure;
            //
            // if (Time.time - _startTime >= blackboard.PatrolTimer)
            // {
            //     _isPatrolling = false;
            //     _isStarted = false;
            //     blackboard.NavMeshAgent.isStopped = true; // NavMeshAgent를 멈춤
            //     blackboard.NavMeshAgent.ResetPath(); // 현재 경로를 초기화
            //     return state = NodeState.Failure;
            // }
            //     
            // if (blackboard.NavMeshAgent.remainingDistance <= blackboard.NavMeshAgent.stoppingDistance)
            // {
            //     _postPatrolIndex = _currentPatrolIndex; // 현재 인덱스를 저장
            //     // 다음 경로 지점으로 이동
            //     _currentPatrolIndex = (_currentPatrolIndex + 1) % blackboard.PatrolPath.Length;
            //     if (_currentPatrolIndex == _postPatrolIndex)
            //         // 다음 경로 지점이 현재 지점과 같으면, 다시 시작
            //         _currentPatrolIndex = (_currentPatrolIndex + 1) % blackboard.PatrolPath.Length;
            // }
            //     
            // var targetPosition = blackboard.PatrolPath[_currentPatrolIndex];
            // blackboard.NavMeshAgent.SetDestination(targetPosition);
            //     
            // blackboard.State = MonsterState.Patrol;
            
            // patrol 명령을 행동 대기열에 추가
            context.Enqueue(new PatrolCommand());
            
            return state = NodeState.Running;
        }
    }
}