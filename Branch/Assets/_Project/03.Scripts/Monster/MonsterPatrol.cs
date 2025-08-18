using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monster
{
    public class MonsterPatrol : MonoBehaviour
    {
        [SerializeField] private List<Transform> patrolPoints;  // 순찰 지점 리스트
        [SerializeField] private float minTime;
        [SerializeField] private float maxTime;
        
        private Transform _target;
        private int _index;
        private float _time;

        public int GenIndex()
        {
            var getIndex = Random.Range(0, patrolPoints.Count);
            if (getIndex == _index)
                return ++_index;
            return _index = getIndex;
        }

        public float GenTime()
        {
            return _time = Random.Range(minTime, maxTime);
        }

        public Vector3 Patrol()
        {
            // _time = GenTime();      // 이동 시간을 설정
            // _index = GenIndex();    // 목적지 번호 생성
            
            _target = patrolPoints[_index]; // 목적지 저장
            
            return _target.position;
            
            // yield break;
        }
    }
}