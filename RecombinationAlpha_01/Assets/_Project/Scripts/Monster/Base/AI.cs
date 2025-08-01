using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Script.CustomCollections;

namespace Monster
{
    public abstract class AI : MonoBehaviour
    {
        // FSM
        private State _currentState;
        
        // 우선순위 큐 (최소 힙 방식)
        private PriorityQueue<Action> _actionQueue = PriorityQueue<Action>.CreateMinPriorityQueue();
        private Action _currentAction;
        private bool _isActing;
        
        // 코루틴 실행을 위한 변수
        private Coroutine _currentCoroutine;
        private Coroutine _previousCoroutine;
        
        [Header("Monster Settings")]
        [SerializeField] private Stats stats;               // 기획자가 정의하는 몬스터의 상태값(구글 스프래드 시트에서 불러올 예정)
        [SerializeField] private Transform body;            // 몬스터의 모델
        [SerializeField] private Animator animator;         // 몬스터 바디에 등록된 애니메이션
        [SerializeField] private NavMeshAgent navMeshAgent; // 길찾기 네비메시 에이전트
        [SerializeField] private Rigidbody rigid;           // 물리 기능
        [SerializeField] private Collider col;              // 충돌 처리
        [SerializeField] private GameObject target;         // 전투 대상

        #region Getter, Setter

        #region Getters

        public Stats Stats => stats;
        public GameObject Target => target;
        public Transform Body => body;
        public Animator Animator => animator;
        public NavMeshAgent NavMeshAgent => navMeshAgent;
        public Rigidbody Rigid => rigid;
        public Collider Col => col;
        public Coroutine CurrentCoroutine => _currentCoroutine;
        public Coroutine PreviousCoroutine => _previousCoroutine;

        #endregion
        public State CurrentState
        {
            get => _currentState;
            set => _currentState = value;
        }
        public bool IsActing
        {
            get =>  _isActing;
            set => _isActing = value;
        }

        #endregion

        #region Unity Methods
        
        private void Update()
        {
            // Debug.Log("Current State: " + _currentState);
            _actionQueue.Clear();                               // 대기열 초기화

            _currentState.OnUpdate();                           // 상태에 해당하는 판단 진행

            if (_isActing || _actionQueue.Count <= 0) return;   // 대기열이 없거나 이미 활동 중이면 종료
            
            _currentAction = _actionQueue.Dequeue();            // 대기열에서 가장 우선순위가 높은 활동을 현재 활동으로 설정
                
            ExecuteAction(_currentAction);                      // 액션 실행
            
            // DebugM();
        }

        #endregion

        #region Public Methods

        // 상태 변경 메서드
        public void ChangeState(State newState)
        {
            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
        }

        // 우선순위 큐에 행동을 추가하는 메서드
        public void AddAction(Action action)
        {
            _actionQueue.Enqueue(action, -action.Priority);
        }
        
        public bool CheckProperties()
        {
            if (!stats.CheckProperties()) return false;
            if (!animator) return false;
            if (!navMeshAgent) return false;
            if (!col) return false;
            // if (!target) return false;
            if (!rigid) return false;
            
            return true;
        }

        public void SetTarget()
        {
            target = MonsterManager.instance.Player;
        }

        public abstract void TakeDamage(int damage);

        #endregion

        // 행동 실행 로직
        private void ExecuteAction(Action action)
        {
            _isActing = true;
            // Debug.Log($"Executing Action: {action.Type} with Priority: {action.Priority}");
            _previousCoroutine = _currentCoroutine;

            switch (action.Type)
            {
                case ActionType.Attack:
                    // 공격 애니메이션, 데미지 처리 등
                    _currentCoroutine = StartCoroutine(AttackCoroutine());
                    break;
                case ActionType.Evasion:
                    _currentCoroutine = StartCoroutine(EvasionCoroutine());
                    break;
                case ActionType.Patrol:
                    _currentCoroutine = StartCoroutine(PatrolCoroutine());
                    break;
                case ActionType.Idle:
                    _currentCoroutine = StartCoroutine(IdleCoroutine());
                    break;
                case ActionType.Chase:
                    _currentCoroutine = StartCoroutine(ChaseCoroutine());
                    break;
                case ActionType.Spawn:
                    // Debug.Log("Spawn Action");
                    _currentCoroutine = StartCoroutine(SpawnCoroutine());
                    break;
                case ActionType.Hit:
                    _currentCoroutine = StartCoroutine(HitCoroutine());
                    break;
                case ActionType.Death:
                    _currentCoroutine = StartCoroutine(DeathCoroutine());
                    break;
            }
        }

        #region CoroutineActions

        // 예시: 행동이 끝나면 isActing을 false로 만들어 다음 행동을 받을 수 있게 함
        protected abstract IEnumerator AttackCoroutine();
        // {
        //     // 공격 애니메이션 시간 등
        //     yield return new WaitForSeconds(2.0f);
        //     _isActing = false;
        // }

        protected abstract IEnumerator SpawnCoroutine();
        // {
        //     yield return new WaitForSeconds(2.0f);
        //     _isActing = false;
        // }
        
        protected abstract IEnumerator EvasionCoroutine();
        protected abstract IEnumerator PatrolCoroutine();
        protected abstract IEnumerator IdleCoroutine();
        protected abstract IEnumerator ChaseCoroutine();
        protected abstract IEnumerator HitCoroutine();
        protected abstract IEnumerator DeathCoroutine();

        #endregion
        
        private void DebugM()
        {
            Debug.Log("============================");
            Debug.Log(_currentAction);
            Debug.Log(_currentState);
            Debug.Log(_actionQueue);
            Debug.Log("============================");
        }
    }
}