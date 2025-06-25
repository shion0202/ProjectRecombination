using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class Monster : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private float hp = 10;
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float attackDistance = 1.0f;
    [SerializeField] private float detectionDistance = 5.0f;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackCooldown = 2.0f; // 공격 쿨타임

    #endregion
    
    #region Cashe Fields

    private Transform _target;
    private NavMeshAgent _agent;
    private SphereCollider _collider;
    // private bool _isTargetInRange;
    private State _state;
    private bool isAttacking = false;
    private Animator _animator;

    #endregion

    enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hitting,
        Die
    }
    
    #region Unity Methods

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = speed;
        
        var rb = GetComponent<Rigidbody>();
        if (rb is not null)
            rb.isKinematic = true;
        
        _animator = GetComponent<Animator>();
        
        _state = State.Idle;
        CreateDetectionTrigger();
    }

    private void Update()
    {
        if (_state == State.Die)
        {
            return;
        }

        if (_state == State.Chase && _target is not null)
        {
            var distanceToTarget = Vector3.Distance(transform.position, _target.position);
            if (distanceToTarget <= attackDistance)
            {
                _state = State.Attack;
                _agent.ResetPath();
            }
            AnimaStateChange(true, false);
        }
        else if (_state == State.Attack && _target is not null)
        {
            // 공격 로직을 여기에 추가
            // Debug.Log("Attacking the target!");

            if (!isAttacking)
            {
                StartCoroutine(Attack());
            }
        }
        else if (_state == State.Idle)
        {
            // 대기 상태에서 할 일 (예: 애니메이션 변경 등)
            _agent.ResetPath();
            AnimaStateChange(false, false);
        }
        else if (_state == State.Patrol)
        {
            // 순찰 로직을 여기에 추가
            // 예: _agent.SetDestination(GetNextPatrolPoint());
            AnimaStateChange(true, false);
        }
        else if (_state == State.Hitting)
        {
            return;
        }
    }

    // private void FixedUpdate()
    // {
    //     if (_state is State.Chase && _target is not null)
    //         _agent.SetDestination(_target.position);
    // }

    #endregion
    
    #region Trigger Methods

    private void OnTriggerStay(Collider other)
    {
        if (_state == State.Die)
            return;

        // Debug.Log(other.gameObject.name);
        if (!other.CompareTag("Player")) return;
        _target = other.transform;
        _state = State.Chase;
        if (attackDistance < Vector3.Distance(transform.position, _target.position) && _state != State.Attack)
            _agent.SetDestination(_target.position);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (_state == State.Die)
            return;

        // Debug.Log(other.gameObject.name);
        if (!other.CompareTag("Player")) return;
        _state = State.Idle;
        _target = null;
        _agent.ResetPath();
    }
    
    #endregion

    #region Private Methods

    private void CreateDetectionTrigger()
    {
        var triggerObj = new GameObject("DetectionTrigger");
        triggerObj.transform.SetParent(transform);
        triggerObj.transform.localPosition = Vector3.zero;
        
        _collider = triggerObj.AddComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = detectionDistance;
        
        triggerObj.layer = LayerMask.NameToLayer("DetectionCollider");
        // triggerObj.tag
    }
    
    private void Die()
    {
        Debug.Log("Monster died!");
        // 죽음 애니메이션이나 효과를 여기에 추가
        _state = State.Die;
        _animator.SetBool("isDie", true);
        // Destroy(gameObject, 5.0f);
    }

    private void AnimaStateChange(bool isWalking, bool isAttack)
    {
        if (_animator is null) return;
        _animator.SetBool("isWalking", isWalking);
        _animator.SetBool("isAttack", isAttack);
    }

    #endregion
    
    #region Coroutines
    
    private IEnumerator Attack()
    {
        if (_state != State.Die)
        {
            isAttacking = true;
            AnimaStateChange(false, true); // 공격 애니메이션 시작
            Debug.Log($"Player took {attackDamage} damage!");
            _target.GetComponent<Player>().TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackCooldown);
            _state = State.Chase; // 공격 후 다시 추적 상태로 전환
            AnimaStateChange(true, false); // 공격 애니메이션 시작
            isAttacking = false;
        }
    }
    
    #endregion

    #region Public Methods

    public void TakeDamage(float damage)
    {
        if (_state == State.Die)
            return;

        hp -= damage;
        Debug.Log($"Monster took {damage} damage! Remaining HP: {hp}");
        if (_animator is not null)
            _animator.Play("Hit");
        if (hp <= 0)
        {
            Die();
        }
    }

    #endregion
}
