using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
public abstract class EnemyBaseController : MonoBehaviour, IDamageable
{
    [Header("Setup")]
    [SerializeField] private EnemyStatsSO _defaultStats;
    [SerializeField] private Transform _target;
    [SerializeField] private Animator _animator;

    [Header("Animation Timings")]
    [SerializeField] private float _hurtDuration = 0.4f;
    [SerializeField] private float _attackDuration = 0.6f;

    [Header("Movement Factors")]
    [SerializeField] protected float _walkFactor = 0.5f;
    [SerializeField] protected float _runFactor = 1f;
    [SerializeField] protected float _dragFactor = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = true;

    private EnemyStatsSO _stats;
    private EnemyStateMachine _stateMachine;

    private float _currentHealth;
    private float _lastAttackTime;

    private bool _isHurt;
    private bool _isAttacking;
    private float _hurtTimer;
    private float _attackTimer;

    public EnemyStatsSO Stats => _stats;
    public Transform Target => _target;
    public float CurrentHealth => _currentHealth;
    public float HealthPercent => _stats == null || _stats.MaxHealth <= 0f ? 0f : _currentHealth / _stats.MaxHealth;
    public bool IsDead => _currentHealth <= 0f;
    public bool CanMove => !IsDead && !_isHurt && !_isAttacking;

    protected virtual void Awake()
    {
        _stateMachine = GetComponent<EnemyStateMachine>();
    }

    protected virtual void Start()
    {
        if (_stats == null && _defaultStats != null)
        {
            Initialize(_defaultStats);
        }

        if (_target == null)
        {
            FindPlayerTarget();
        }
    }

    protected virtual void Update()
    {
        UpdateTimers();

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            UpdateAnimatorSpeed(0f);
            return;
        }

        if (_target == null)
        {
            FindPlayerTarget();
            UpdateAnimatorSpeed(0f);
            return;
        }

        _stateMachine.Tick();
    }

    public virtual void Initialize(EnemyStatsSO stats)
    {
        _stats = stats;
        _currentHealth = _stats.MaxHealth;
        _lastAttackTime = -999f;

        _isHurt = false;
        _isAttacking = false;
        _hurtTimer = 0f;
        _attackTimer = 0f;

        UpdateAnimatorSpeed(0f);
        SetDraggingState(ShouldDrag());
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead || _stats == null)
            return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        if (IsDead)
        {
            Die();
            return;
        }

        _isHurt = true;
        _hurtTimer = _hurtDuration;

        _isAttacking = false;
        _attackTimer = 0f;

        TriggerHurtAnimation();
        SetDraggingState(ShouldDrag());
        UpdateAnimatorSpeed(0f);
    }

    public bool IsTargetInAttackRange()
    {
        if (_target == null || _stats == null)
            return false;

        return Vector3.Distance(transform.position, _target.position) <= _stats.AttackRange;
    }

    public bool HasRecoveredSafeDistance()
    {
        if (_target == null || _stats == null)
            return false;

        return Vector3.Distance(transform.position, _target.position) >= _stats.RecoverDistance;
    }

    public virtual void RecoverAndResume()
    {
        if (_stats == null || IsDead)
            return;

        _currentHealth = _stats.MaxHealth;
        _isHurt = false;
        _isAttacking = false;
        _hurtTimer = 0f;
        _attackTimer = 0f;

        SetDraggingState(false);
        UpdateAnimatorSpeed(0f);
    }

    public bool CanAttack()
    {
        if (_stats == null || IsDead || _isHurt || _isAttacking)
            return false;

        return Time.time >= _lastAttackTime + _stats.AttackCooldown;
    }

    public virtual void Attack()
    {
        if (_target == null || _stats == null || IsDead)
            return;

        if (!CanAttack())
            return;

        _lastAttackTime = Time.time;
        _isAttacking = true;
        _attackTimer = _attackDuration;

        UpdateAnimatorSpeed(0f);
        TriggerAttackAnimation();

        if (_target.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_stats.Damage);
        }
    }

    public virtual void StopMovingAndFaceTarget()
    {
        if (_target == null || IsDead)
        {
            UpdateAnimatorSpeed(0f);
            return;
        }

        UpdateAnimatorSpeed(0f);

        if (!CanMove)
            return;

        Vector3 lookDirection = _target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    public abstract void MoveByState();
    public abstract void Flee();
    public abstract bool ShouldFlee();
    public abstract bool ShouldRecoverFromFlee();
    protected abstract float GetChaseMoveFactor();
    protected abstract bool ShouldDrag();

    protected void MoveDirect(float moveFactor)
    {
        if (_target == null || _stats == null)
            return;

        float finalSpeed = _stats.MoveSpeed * moveFactor;
        MoveTowards(_target.position, finalSpeed, true, moveFactor);
    }

    protected void MoveZigZag(float moveFactor)
    {
        if (_target == null || _stats == null)
            return;

        Vector3 toTarget = _target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.001f)
        {
            UpdateAnimatorSpeed(0f);
            return;
        }

        Vector3 forward = toTarget.normalized;
        Vector3 lateral = Vector3.Cross(Vector3.up, forward).normalized;
        float wave = Mathf.Sin(Time.time * _stats.ZigZagFrequency) * _stats.ZigZagAmplitude;

        Vector3 destination = _target.position + lateral * wave;
        float finalSpeed = _stats.MoveSpeed * moveFactor;

        MoveTowards(destination, finalSpeed, true, moveFactor);
    }

    protected void MoveToBackstabPosition(float moveFactor)
    {
        if (_target == null || _stats == null)
            return;

        Vector3 behindTarget = _target.position - (_target.forward * _stats.BackstabDistance);
        float finalSpeed = _stats.MoveSpeed * moveFactor;

        MoveTowards(behindTarget, finalSpeed, true, moveFactor);
    }

    protected void MoveAwayFromTarget(float moveFactor)
    {
        if (_target == null || _stats == null)
            return;

        Vector3 fleeDirection = (transform.position - _target.position).normalized;
        Vector3 destination = transform.position + fleeDirection * _stats.FleeDistance;
        float finalSpeed = _stats.MoveSpeed * moveFactor;

        MoveTowards(destination, finalSpeed, true, moveFactor);
    }

    protected void MoveTowards(Vector3 destination, float finalSpeed, bool faceMovement, float moveFactor)
    {
        if (_stats == null)
            return;

        Vector3 moveDirection = destination - transform.position;
        moveDirection.y = 0f;

        float distance = moveDirection.magnitude;

        if (distance <= _stats.StoppingDistance)
        {
            UpdateAnimatorSpeed(0f);
            return;
        }

        Vector3 normalizedDirection = moveDirection.normalized;
        transform.position += normalizedDirection * finalSpeed * Time.deltaTime;

        if (faceMovement && normalizedDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(normalizedDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        UpdateAnimatorSpeed(moveFactor);
        SetDraggingState(ShouldDrag());
    }

    protected void UpdateAnimatorSpeed(float speedValue)
    {
        if (_animator == null)
            return;

        _animator.SetFloat("Speed", speedValue);
    }

    protected void SetDraggingState(bool isDragging)
    {
        if (_animator == null)
            return;

        _animator.SetBool("IsDragging", isDragging);
    }

    protected void TriggerAttackAnimation()
    {
        if (_animator == null)
            return;

        _animator.ResetTrigger("Hurt");
        _animator.SetTrigger("Attack");
    }

    protected void TriggerHurtAnimation()
    {
        if (_animator == null)
            return;

        _animator.ResetTrigger("Attack");
        _animator.SetTrigger("Hurt");
    }

    protected void TriggerDeathAnimation()
    {
        if (_animator == null)
            return;

        _animator.ResetTrigger("Attack");
        _animator.ResetTrigger("Hurt");
        _animator.SetTrigger("Dead");
    }

    private void UpdateTimers()
    {
        if (_isHurt)
        {
            _hurtTimer -= Time.deltaTime;
            if (_hurtTimer <= 0f)
            {
                _isHurt = false;
            }
        }

        if (_isAttacking)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                _isAttacking = false;
            }
        }
    }

    protected virtual void Die()
    {
        _currentHealth = 0f;
        _isHurt = false;
        _isAttacking = false;

        UpdateAnimatorSpeed(0f);
        SetDraggingState(false);
        TriggerDeathAnimation();

        _stateMachine.SetState(EnemyState.Dead);

        ScoreManager.Instance?.AddScore(_stats.ScoreOnDeath);

        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Destroy(gameObject, 1.5f);
    }

    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            _target = player.transform;
            return;
        }

        if (Camera.main != null)
        {
            _target = Camera.main.transform.root;
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (!_drawGizmos || _stats == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stats.AttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _stats.StoppingDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _stats.RecoverDistance);
    }
}