using UnityEngine;

public class PistolWeapon : ProjectileWeaponBase
{
    protected override void ExecuteShoot()
    {
        SpawnProjectile(_shootPoint.forward);
    }
}