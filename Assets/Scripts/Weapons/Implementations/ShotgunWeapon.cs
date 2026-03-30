using UnityEngine;

public class ShotgunWeapon : ProjectileWeaponBase
{
    protected override void ExecuteShoot()
    {
        int projectileCount = Mathf.Max(1, _data.ProjectileCount);

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 direction = GetSpreadDirection(_shootPoint.forward, _data.SpreadAngle);
            SpawnProjectile(direction);
        }
    }

    private Vector3 GetSpreadDirection(Vector3 baseDirection, float spreadAngle)
    {
        Vector3 spreadRotation = new Vector3(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0f);

        return Quaternion.Euler(spreadRotation) * baseDirection;
    }
}