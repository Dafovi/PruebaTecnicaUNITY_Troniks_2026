using UnityEngine;

public class MachineGunWeapon : ProjectileWeaponBase
{
    [SerializeField] private float _randomSpread = 2f;

    protected override void ExecuteShoot()
    {
        Vector3 direction = GetSpreadDirection(_shootPoint.forward, _randomSpread);
        SpawnProjectile(direction);
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