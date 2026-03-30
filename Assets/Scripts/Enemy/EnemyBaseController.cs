using System;
using UnityEngine;

[RequireComponent(typeof(EnemyStateMachine))]
public abstract class EnemyBaseController : MonoBehaviour, IDamageable
{
    public static Action<EnemyBaseController> OnEnemyDied;

    [Header("Setup")]
    [SerializeField] private EnemyStatsSO _defaultStats;
    [SerializeField] private Transform _target;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _animatedRoot;

    [Header("Animation Timings")]
    [SerializeField] private float _hurtDuration = 0.4f;
    [SerializeField] private float _attackDuration = 0.6f;

    [Header("Movement Factors")]
    [SerializeField] protected float _walkFactor = 0.5f;
    [SerializeField] protected float _runFactor = 1f;
    [SerializeField] protected float _dragFactor = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = true;

    private EnemyStateMachine _stateMachine;
    private Collider[] _cachedColliders;

    private EnemyStatsSO _baseStats;

    private float _moveSpeed;
    private int _damage;
    private float _attackCooldown;
    private float _maxHealth;
    private float _attackRange;
    private float _stoppingDistance;
    private int _scoreOnDeath;
    private float _zigZagAmplitude;
    private float _zigZagFrequency;
    private float _backstabDistance;
    private float _fleeDistance;
    private float _recoverDistance;

    private float _currentHealth;
    private float _lastAttackTime;

    private bool _isHurt;
    private bool _isAttacking;
    private float _hurtTimer;
    private float _attackTimer;
    private bool _deathNotified;

    private string _poolKey;
    private GameObject _originPrefab;

    private Vector3 _cachedAnimatedRootLocalPosition;
    private Quaternion _cachedAnimatedRootLocalRotation;
    private Vector3 _cachedAnimatedRootLocalScale;
    private bool _hasCachedAnimatedRootTransform;

    public EnemyStatsSO DefaultStatsAsset => _defaultStats;
    public Transform Target => _target;

    public float MoveSpeed => _moveSpeed;
    public int Damage => _damage;
    public float AttackCooldown => _attackCooldown;
    public float MaxHealth => _maxHealth;
    public float AttackRange => _attackRange;
    public float StoppingDistance => _stoppingDistance;
    public int ScoreOnDeath => _scoreOnDeath;
    public float ZigZagAmplitude => _zigZagAmplitude;
    public float ZigZagFrequency => _zigZagFrequency;
    public float BackstabDistance => _backstabDistance;
    public float FleeDistance => _fleeDistance;
    public float RecoverDistance => _recoverDistance;

    public float CurrentHealth => _currentHealth;
    public float HealthPercent => _maxHealth <= 0f ? 0f : _currentHealth / _maxHealth;
    public bool IsDead => _currentHealth <= 0f;
    public bool CanMove => !IsDead && !_isHurt && !_isAttacking;

    protected virtual void Awake()
    {
        _stateMachine = GetComponent<EnemyStateMachine>();
        _cachedColliders = GetComponentsInChildren<Collider>(true);

        CacheAnimatedRootReference();
        CacheAnimatedRootTransform();
    }

    protected virtual void Start()
    {
        if (_baseStats == null && _defaultStats != null)
        {
            SpawnFromPool(_defaultStats, _target, 1f, 1f, 1f);
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

    public void SetPoolData(string poolKey, GameObject originPrefab)
    {
        _poolKey = poolKey;
        _originPrefab = originPrefab;
    }

    public virtual void SpawnFromPool(
        EnemyStatsSO baseStats,
        Transform target,
        float healthMultiplier,
        float damageMultiplier,
        float speedMultiplier)
    {
        _baseStats = baseStats;

        _moveSpeed = baseStats.MoveSpeed * speedMultiplier;
        _damage = Mathf.Max(1, Mathf.RoundToInt(baseStats.Damage * damageMultiplier));
        _attackCooldown = baseStats.AttackCooldown;
        _maxHealth = baseStats.MaxHealth * healthMultiplier;
        _attackRange = baseStats.AttackRange;
        _stoppingDistance = baseStats.StoppingDistance;
        _scoreOnDeath = baseStats.ScoreOnDeath;

        _zigZagAmplitude = baseStats.ZigZagAmplitude;
        _zigZagFrequency = baseStats.ZigZagFrequency;

        _backstabDistance = baseStats.BackstabDistance;
        _fleeDistance = baseStats.FleeDistance;
        _recoverDistance = baseStats.RecoverDistance;

        _currentHealth = _maxHealth;
        _lastAttackTime = -999f;

        _isHurt = false;
        _isAttacking = false;
        _hurtTimer = 0f;
        _attackTimer = 0f;
        _deathNotified = false;

        SetTarget(target);
        EnableAllColliders(true);

        RestoreAnimatedRootTransform();
        ResetAnimatorState();

        UpdateAnimatorSpeed(0f);
        SetDraggingState(ShouldDrag());
    }

    public void ReturnToPool()
    {
        if (PoolManager.Instance == null || string.IsNullOrWhiteSpace(_poolKey) || _originPrefab == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnableAllColliders(true);
        PoolManager.Instance.ReturnObject(_poolKey, _originPrefab, gameObject);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead)
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
        if (_target == null)
            return false;

        return Vector3.Distance(transform.position, _target.position) <= _attackRange;
    }

    public bool HasRecoveredSafeDistance()
    {
        if (_target == null)
            return false;

        return Vector3.Distance(transform.position, _target.position) >= _recoverDistance;
    }

    public virtual void RecoverAndResume()
    {
        if (IsDead)
            return;

        _currentHealth = _maxHealth;
        _isHurt = false;
        _isAttacking = false;
        _hurtTimer = 0f;
        _attackTimer = 0f;

        SetDraggingState(false);
        UpdateAnimatorSpeed(0f);
    }

    public bool CanAttack()
    {
        if (IsDead || _isHurt || _isAttacking)
            return false;

        return Time.time >= _lastAttackTime + _attackCooldown;
    }

    public virtual void Attack()
    {
        if (_target == null || IsDead)
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
            damageable.TakeDamage(_damage);
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
    protected abstract bool ShouldDrag();

    protected void MoveDirect(float moveFactor)
    {
        if (_target == null)
            return;

        float finalSpeed = _moveSpeed * moveFactor;
        MoveTowards(_target.position, finalSpeed, true, moveFactor);
    }

    protected void MoveZigZag(float moveFactor)
    {
        if (_target == null)
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
        float wave = Mathf.Sin(Time.time * _zigZagFrequency) * _zigZagAmplitude;

        Vector3 destination = _target.position + lateral * wave;
        float finalSpeed = _moveSpeed * moveFactor;

        MoveTowards(destination, finalSpeed, true, moveFactor);
    }

    protected void MoveToBackstabPosition(float moveFactor)
    {
        if (_target == null)
            return;

        Vector3 behindTarget = _target.position - (_target.forward * _backstabDistance);
        float finalSpeed = _moveSpeed * moveFactor;

        MoveTowards(behindTarget, finalSpeed, true, moveFactor);
    }

    protected void MoveAwayFromTarget(float moveFactor)
    {
        if (_target == null)
            return;

        Vector3 fleeDirection = (transform.position - _target.position).normalized;
        Vector3 destination = transform.position + fleeDirection * _fleeDistance;
        float finalSpeed = _moveSpeed * moveFactor;

        MoveTowards(destination, finalSpeed, true, moveFactor);
    }

    protected void MoveTowards(Vector3 destination, float finalSpeed, bool faceMovement, float moveFactor)
    {
        Vector3 moveDirection = destination - transform.position;
        moveDirection.y = 0f;

        float distance = moveDirection.magnitude;

        if (distance <= _stoppingDistance)
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
        EnableAllColliders(false);

        if (!_deathNotified)
        {
            _deathNotified = true;
            //ScoreManager.Instance?.AddScore(_scoreOnDeath);
            OnEnemyDied?.Invoke(this);
        }
    }

    private void CacheAnimatedRootReference()
    {
        if (_animatedRoot != null)
            return;

        if (_animator != null)
        {
            _animatedRoot = _animator.transform;
        }
    }

    private void CacheAnimatedRootTransform()
    {
        if (_animatedRoot == null)
            return;

        _cachedAnimatedRootLocalPosition = _animatedRoot.localPosition;
        _cachedAnimatedRootLocalRotation = _animatedRoot.localRotation;
        _cachedAnimatedRootLocalScale = _animatedRoot.localScale;
        _hasCachedAnimatedRootTransform = true;
    }

    private void RestoreAnimatedRootTransform()
    {
        if (_animatedRoot == null || !_hasCachedAnimatedRootTransform)
            return;

        _animatedRoot.localPosition = _cachedAnimatedRootLocalPosition;
        _animatedRoot.localRotation = _cachedAnimatedRootLocalRotation;
        _animatedRoot.localScale = _cachedAnimatedRootLocalScale;
    }

    private void ResetAnimatorState()
    {
        if (_animator == null)
            return;

        _animator.Rebind();
        _animator.Update(0f);
    }

    private void EnableAllColliders(bool enabled)
    {
        if (_cachedColliders == null)
            return;

        for (int i = 0; i < _cachedColliders.Length; i++)
        {
            _cachedColliders[i].enabled = enabled;
        }
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
        if (!_drawGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _stoppingDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _recoverDistance);
    }
}