using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("General")]
    public float MoveSpeed = 2.2f;
    public int Damage = 1;
    public float AttackCooldown = 1f;
    public float MaxHealth = 10f;
    public float AttackRange = 1.2f;
    public float StoppingDistance = 0.9f;
    public int ScoreOnDeath = 10;

    [Header("Aggressive")]
    public float ZigZagAmplitude = 1f;
    public float ZigZagFrequency = 3f;

    [Header("Flanker")]
    public float BackstabDistance = 2.5f;
    public float FleeDistance = 5f;
    public float RecoverDistance = 6f;
}