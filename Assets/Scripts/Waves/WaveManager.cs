using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WaveProgressionSO _progression;
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private float _timeBetweenWaves = 1.5f;

    private int _currentWaveNumber;
    private int _enemiesToSpawnThisWave;
    private int _enemiesSpawnedThisWave;
    private int _aliveEnemiesThisWave;

    private float _spawnTimer;
    private float _currentSpawnInterval;
    private bool _waveRunning;
    private bool _waitingNextWave;

    private readonly List<EnemyBaseController> _spawnedEnemiesThisWave = new List<EnemyBaseController>();

    private void OnEnable()
    {
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnReturnToMenu += HandleReturnToMenu;
        EnemyBaseController.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnReturnToMenu -= HandleReturnToMenu;
        EnemyBaseController.OnEnemyDied -= HandleEnemyDied;
    }

    private void Update()
    {
        if (!_waveRunning || _waitingNextWave)
            return;

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (_enemiesSpawnedThisWave < _enemiesToSpawnThisWave)
        {
            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer <= 0f)
            {
                SpawnEnemy();
                _spawnTimer = _currentSpawnInterval;
            }
        }

        if (_enemiesSpawnedThisWave >= _enemiesToSpawnThisWave && _aliveEnemiesThisWave <= 0)
        {
            StartCoroutine(AdvanceToNextWaveRoutine());
        }
    }

    public void StartWaves()
    {
        CleanupCurrentWaveEnemies();

        _currentWaveNumber = 0;
        _waitingNextWave = false;
        _waveRunning = false;

        StartNextWave();
    }

    private void HandleGameStarted()
    {
        StartWaves();
    }

    private void HandleReturnToMenu()
    {
        _waveRunning = false;
        _waitingNextWave = false;
        CleanupCurrentWaveEnemies();
    }

    private void StartNextWave()
    {
        _currentWaveNumber++;

        _enemiesToSpawnThisWave = GetEnemyCountForWave(_currentWaveNumber);
        _enemiesSpawnedThisWave = 0;
        _aliveEnemiesThisWave = 0;
        _spawnTimer = 0f;
        _currentSpawnInterval = GetSpawnIntervalForWave(_currentWaveNumber);
        _waveRunning = true;
        _waitingNextWave = false;

        GameEvents.OnWaveChanged?.Invoke(_currentWaveNumber);
    }

    private void SpawnEnemy()
    {
        if (_progression == null || _progression.Enemies == null || _progression.Enemies.Count == 0 || _spawnPoints.Count == 0)
            return;

        EnemySpawnData spawnData = GetRandomEnemyDataForWave(_currentWaveNumber);
        if (spawnData == null || spawnData.EnemyPrefab == null)
            return;

        EnemyStatsSO baseStats = spawnData.BaseStats != null
            ? spawnData.BaseStats
            : spawnData.EnemyPrefab.DefaultStatsAsset;

        if (baseStats == null)
            return;

        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];

        GameObject prefabObject = spawnData.EnemyPrefab.gameObject;
        string poolKey = GetEnemyPoolKey(prefabObject);

        GameObject enemyObject = PoolManager.Instance.GetObject(
            poolKey,
            prefabObject,
            spawnPoint.position,
            spawnPoint.rotation);

        EnemyBaseController enemy = enemyObject.GetComponent<EnemyBaseController>();
        if (enemy == null)
            return;

        enemy.SetPoolData(poolKey, prefabObject);
        enemy.SpawnFromPool(
            baseStats,
            _playerTarget != null ? _playerTarget : FindPlayerTarget(),
            GetHealthMultiplierForWave(_currentWaveNumber),
            GetDamageMultiplierForWave(_currentWaveNumber),
            GetMoveSpeedMultiplierForWave(_currentWaveNumber));

        _enemiesSpawnedThisWave++;
        _aliveEnemiesThisWave++;
        _spawnedEnemiesThisWave.Add(enemy);
    }

    private void HandleEnemyDied(EnemyBaseController enemy)
    {
        if (!_waveRunning || enemy == null)
            return;

        _aliveEnemiesThisWave = Mathf.Max(0, _aliveEnemiesThisWave - 1);
    }

    private IEnumerator AdvanceToNextWaveRoutine()
    {
        if (_waitingNextWave)
            yield break;

        _waitingNextWave = true;
        _waveRunning = false;

        yield return new WaitForSeconds(_timeBetweenWaves);

        CleanupCurrentWaveEnemies();
        StartNextWave();
    }

    private void CleanupCurrentWaveEnemies()
    {
        for (int i = 0; i < _spawnedEnemiesThisWave.Count; i++)
        {
            if (_spawnedEnemiesThisWave[i] != null)
            {
                _spawnedEnemiesThisWave[i].ReturnToPool();
            }
        }

        _spawnedEnemiesThisWave.Clear();
    }

    private int GetEnemyCountForWave(int waveNumber)
    {
        return _progression.BaseEnemyCount + ((waveNumber - 1) * _progression.EnemyCountIncreasePerWave);
    }

    private float GetSpawnIntervalForWave(int waveNumber)
    {
        float interval = _progression.BaseSpawnInterval - ((waveNumber - 1) * _progression.SpawnIntervalDecreasePerWave);
        return Mathf.Max(_progression.MinimumSpawnInterval, interval);
    }

    private float GetHealthMultiplierForWave(int waveNumber)
    {
        return 1f + ((waveNumber - 1) * _progression.HealthMultiplierPerWave);
    }

    private float GetDamageMultiplierForWave(int waveNumber)
    {
        return 1f + ((waveNumber - 1) * _progression.DamageMultiplierPerWave);
    }

    private float GetMoveSpeedMultiplierForWave(int waveNumber)
    {
        return 1f + ((waveNumber - 1) * _progression.MoveSpeedMultiplierPerWave);
    }

    private EnemySpawnData GetRandomEnemyDataForWave(int waveNumber)
    {
        List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>();

        for (int i = 0; i < _progression.Enemies.Count; i++)
        {
            EnemySpawnData data = _progression.Enemies[i];

            if (data == null || data.EnemyPrefab == null)
                continue;

            if (waveNumber >= data.UnlockWave)
            {
                availableEnemies.Add(data);
            }
        }

        if (availableEnemies.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < availableEnemies.Count; i++)
        {
            totalWeight += GetWeightForWave(availableEnemies[i], waveNumber);
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < availableEnemies.Count; i++)
        {
            float weight = GetWeightForWave(availableEnemies[i], waveNumber);
            cumulative += weight;

            if (randomValue <= cumulative)
                return availableEnemies[i];
        }

        return availableEnemies[0];
    }

    private float GetWeightForWave(EnemySpawnData data, int waveNumber)
    {
        int waveDelta = Mathf.Max(0, waveNumber - data.UnlockWave);
        return data.BaseWeight + (waveDelta * data.WeightIncreasePerWave);
    }

    private string GetEnemyPoolKey(GameObject prefabObject)
    {
        return $"{prefabObject.name}_Enemies";
    }

    private Transform FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            return player.transform;

        if (Camera.main != null)
            return Camera.main.transform.root;

        return null;
    }
}