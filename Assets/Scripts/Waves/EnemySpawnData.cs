using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyBaseController EnemyPrefab;
    public EnemyStatsSO BaseStats;
    public int UnlockWave = 1;
    [Min(0f)] public float BaseWeight = 1f;
    [Min(0f)] public float WeightIncreasePerWave = 0f;
}