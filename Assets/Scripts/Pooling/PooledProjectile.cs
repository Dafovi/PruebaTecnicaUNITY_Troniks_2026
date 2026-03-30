using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PooledProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    private string _poolKey;
    private GameObject _originPrefab;
    private float _speed;
    private int _damage;
    private float _lifeTime;
    private float _timer;
    private bool _isInitialized;

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _timer = _lifeTime;
    }

    private void Update()
    {
        if (!_isInitialized)
            return;

        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            ReturnToPool();
        }
    }

    public void SetPoolData(string poolKey, GameObject originPrefab)
    {
        _poolKey = poolKey;
        _originPrefab = originPrefab;
    }

    public void Initialize(string poolKey, GameObject originPrefab, int damage, float speed, float lifeTime, Vector3 direction)
    {
        _poolKey = poolKey;
        _originPrefab = originPrefab;
        _damage = damage;
        _speed = speed;
        _lifeTime = lifeTime;
        _timer = _lifeTime;
        _isInitialized = true;

        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.velocity = direction.normalized * _speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized)
            return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _isInitialized = false;

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        if (_originPrefab == null || PoolManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        PoolManager.Instance.ReturnObject(_poolKey, _originPrefab, gameObject);
    }
}