using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Wave Progression")]
public class WaveProgressionSO : ScriptableObject
{
    [Header("Enemies Per Wave")]
    public int BaseEnemyCount = 4;
    public int EnemyCountIncreasePerWave = 2;

    [Header("Spawn Timing")]
    public float BaseSpawnInterval = 1.25f;
    public float SpawnIntervalDecreasePerWave = 0.05f;
    public float MinimumSpawnInterval = 0.35f;

    [Header("Stat Scaling")]
    public float HealthMultiplierPerWave = 0.12f;
    public float DamageMultiplierPerWave = 0.05f;
    public float MoveSpeedMultiplierPerWave = 0.03f;

    [Header("Enemy Pool")]
    public List<EnemySpawnData> Enemies;
}