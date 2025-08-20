using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Monster;
// using Monster.Skill;
using UnityEngine.Serialization;

namespace AI.Blackboard
{
    public class Blackboard : MonoBehaviour
    {
        #region SerializeFields
        
        [Tooltip("몬스터의 현재 상태를 나타냅니다.")][SerializeField] private MonsterState state = MonsterState.Idle;
        [SerializeField] private int index;
        
        [Header("Dependency")] // 의존성 주입
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private GameObject agent;          // AI 에이전트 (몬스터)
        [SerializeField] private GameObject target;
        
        // [Header("Stats")]
        // [Tooltip("몬스터 기본 세팅")][SerializeField] private MonsterStatsDefault defaultStats;  // 몬스터 초기 상태
        
        [Header("Patrol Setting")][SerializeField] private MonsterPatrol patrolInfo; // 몬스터 순찰 스크립트
        [Header("Wander Setting")][SerializeField] private MonsterWander wanderInfo; // 몬스터 방황 스크립트

        #endregion
        
        #region private Fields
        
        private IEnumerator coroutine = null; // 코루틴을 저장할 변수
        private bool isCoroutineRunning = false; // 코루틴이 실행 중인지 여부를 저장할 변수
        
        private List<(int, float)> _cooldownSkills = new();    // 스킬의 쿨타입을 적용해 사용 가능 여부 관리
        
        #endregion
        
        #region Properties
        
        public MonsterState State
        {
            get => state;
            set => state = value;
        }
        
        public NavMeshAgent NavMeshAgent
        {
            get => navMeshAgent;
            set => navMeshAgent = value;
        }
        
        public GameObject Agent
        {
            get => agent;
            set => agent = value;
        }
        
        public GameObject Target
        {
            get => target;
            set => target = value;
        }
        
        // public MonsterStatsDefault DefaultStats
        // {
        //     get => defaultStats;
        //     set => defaultStats = value;
        // }
        
        public MonsterPatrol PatrolInfo
        {
            get => patrolInfo;
            set => patrolInfo = value;
        }
        
        public MonsterWander WanderInfo
        {
            get => wanderInfo;
            set => wanderInfo = value;
        }

        #endregion

        #region Dictionary

        private readonly Dictionary<string, object> _map = new();
        
        public void Set<T>(BBKey<T> key, T value) => _map[key.Name] = value;

        public bool TryGet<T>(BBKey<T> key, out T value)
        {
            if (_map.TryGetValue(key.Name, out var obj) && obj is T t) 
            { 
                value = t;
                return true;
            }

            value = default!;
            return false;
        }

        #endregion

        #region Methods

        public void InitDefaultValues()
        {
            // 기본값 설정
            // if (defaultStats is null)
            // {
            //     // stats가 설정되지 않은 경우, 기본값을 사용
            //     _map["Health"] = 100f;                  // AI의 현재 체력
            //     _map["MinSpeed"] = 3.5f;                // 걷기 이동 속도
            //     _map["MaxSpeed"] = 5f;                  // 뛰기 이동 속도
            //     _map["Damage"] = 10f;                   // 공격력
            //     _map["AttackSpeed"] = 1f;               // 공격 속도
            //     _map["Defence"] = 5f;                   // 방어력
            //     
            //     _map["DetectionRange"] = 10f;           // 타겟 인식 범위
            //     _map["RotationSpeed"] = 5f;             // 회전 속도
            // }
            // else
            // {
            //     _map["Health"] = defaultStats.MaxHealth;                  // AI의 현재 체력
            //     _map["MinSpeed"] = defaultStats.MinSpeed;                // 걷기 이동 속도
            //     _map["MaxSpeed"] = defaultStats.MaxSpeed;                  // 뛰기 이동 속도
            //     _map["Damage"] = defaultStats.Damage;                   // 공격력
            //     _map["AttackSpeed"] = defaultStats.AttackSpeed;               // 공격 속도
            //     _map["Defence"] = defaultStats.Defence;                   // 방어력
            //     
            //     _map["DetectionRange"] = defaultStats.DetectionRange;           // 타겟 인식 범위
            //     _map["RotationSpeed"] = defaultStats.RotationSpeed;             // 회전 속도
            // }
            
            // 몬스터의 인덱스 번호 확인
            if (index >= 1000 && index <= 1999)
            {
                RowData defaultStats = DataManager.Instance.GetRowDataByIndex("CharacterStats", index);
                
                if (defaultStats == null)
                {
                    Debug.Log("Default stats not found for index: " + index);
                    return;
                }
                // Debug.Log(defaultStats.ToString()); // 디버그용: 기본 스탯 데이터 확인
                
                _map["Health"] = defaultStats.GetStat(EStatType.MaxHp);                  // AI의 현재 체력
                _map["MinSpeed"] = defaultStats.GetStat(EStatType.WalkSpeed);                // 걷기 이동 속도
                _map["MaxSpeed"] = defaultStats.GetStat(EStatType.RunSpeed);                  // 뛰기 이동 속도
                _map["Damage"] = defaultStats.GetStat(EStatType.Damage);                   // 공격력
                _map["Defence"] = defaultStats.GetStat(EStatType.Defence);                   // 방어력
                _map["DetectionRange"] = defaultStats.GetStat(EStatType.Range);           // 타겟 인식 범위
            }
        }

        public void Clear()
        {
            _map.Clear();
        }

        #endregion
    }
}