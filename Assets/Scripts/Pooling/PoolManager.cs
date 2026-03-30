using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    private class Pool
    {
        public string Key;
        public GameObject Prefab;
        public Queue<GameObject> Objects = new Queue<GameObject>();
        public Transform Parent;
    }

    [SerializeField] private int _defaultPrewarmCount = 10;
    [SerializeField] private Transform _poolsRoot;

    private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_poolsRoot == null)
        {
            GameObject poolsRootObject = new GameObject("Pools");
            poolsRootObject.transform.SetParent(transform);
            _poolsRoot = poolsRootObject.transform;
        }
    }

    public GameObject GetObject(string poolKey, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrWhiteSpace(poolKey))
        {
            poolKey = prefab.name;
        }

        if (!_pools.TryGetValue(poolKey, out Pool pool))
        {
            pool = CreatePool(poolKey, prefab, _defaultPrewarmCount);
        }

        GameObject instance = pool.Objects.Count > 0
            ? pool.Objects.Dequeue()
            : CreateInstance(pool);

        instance.transform.SetParent(pool.Parent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        if (instance.TryGetComponent(out PooledProjectile pooledProjectile))
        {
            pooledProjectile.SetPoolData(poolKey, prefab);
        }

        return instance;
    }

    public void ReturnObject(string poolKey, GameObject prefab, GameObject instance)
    {
        if (string.IsNullOrWhiteSpace(poolKey))
        {
            poolKey = prefab.name;
        }

        if (!_pools.TryGetValue(poolKey, out Pool pool))
        {
            pool = CreatePool(poolKey, prefab, 0);
        }

        instance.transform.SetParent(pool.Parent);
        instance.SetActive(false);
        pool.Objects.Enqueue(instance);
    }

    private Pool CreatePool(string poolKey, GameObject prefab, int prewarmCount)
    {
        GameObject parentObject = new GameObject(poolKey);
        parentObject.transform.SetParent(_poolsRoot);

        Pool pool = new Pool
        {
            Key = poolKey,
            Prefab = prefab,
            Parent = parentObject.transform
        };

        _pools.Add(poolKey, pool);

        for (int i = 0; i < prewarmCount; i++)
        {
            GameObject instance = CreateInstance(pool);
            instance.SetActive(false);
            pool.Objects.Enqueue(instance);
        }

        return pool;
    }

    private GameObject CreateInstance(Pool pool)
    {
        GameObject instance = Instantiate(pool.Prefab, pool.Parent);
        instance.name = pool.Prefab.name;
        return instance;
    }
}