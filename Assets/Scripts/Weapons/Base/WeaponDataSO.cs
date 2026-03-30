using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapons/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("General")]
    public float FireRate = 0.25f;
    public int Damage = 1;
    public GameObject ProjectilePrefab;

    [Header("Projectile")]
    public float ProjectileSpeed = 20f;
    public float ProjectileLifetime = 3f;

    [Header("Burst / Spread")]
    public int ProjectileCount = 1;
    public float SpreadAngle = 0f;
}