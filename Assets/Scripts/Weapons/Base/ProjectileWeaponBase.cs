using UnityEngine;

public abstract class ProjectileWeaponBase : WeaponBase
{
    [Header("Projectile Setup")]
    [SerializeField] protected Transform _shootPoint;

    protected void SpawnProjectile(Vector3 direction)
    {
        string poolKey = GetPoolKey();

        GameObject projectileObject = PoolManager.Instance.GetObject(
            poolKey,
            _data.ProjectilePrefab,
            _shootPoint.position,
            Quaternion.LookRotation(direction));

        if (projectileObject.TryGetComponent(out PooledProjectile projectile))
        {
            projectile.Initialize(
                poolKey,
                _data.ProjectilePrefab,
                _data.Damage,
                _data.ProjectileSpeed,
                _data.ProjectileLifetime,
                direction);
        }
    }

    protected virtual string GetPoolKey()
    {
        string weaponName = SanitizeName(gameObject.name);
        string projectileName = _data != null && _data.ProjectilePrefab != null
            ? SanitizeName(_data.ProjectilePrefab.name)
            : "Projectile";

        return $"{weaponName}_{projectileName}_Pool";
    }

    private string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unnamed";

        return value.Replace("(Clone)", "").Trim();
    }
}