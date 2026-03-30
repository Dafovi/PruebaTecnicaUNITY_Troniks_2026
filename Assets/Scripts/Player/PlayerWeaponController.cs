using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private List<WeaponBase> _weapons;
    private int _currentWeaponIndex;

    private WeaponBase CurrentWeapon => _weapons[_currentWeaponIndex];

    private void OnEnable()
    {
        InputController.OnShootPressed += Shoot;
        InputController.OnSwitchWeaponPressed += NextWeapon;
    }

    private void OnDisable()
    {
        InputController.OnShootPressed -= Shoot;
        InputController.OnSwitchWeaponPressed -= NextWeapon;
    }

    private void Start()
    {
        UpdateWeaponVisibility();
    }

    public void Shoot()
    {
        if (_weapons == null || _weapons.Count == 0)
            return;

        CurrentWeapon.Shoot();
    }

    public void NextWeapon()
    {
        if (_weapons == null || _weapons.Count == 0)
            return;

        _currentWeaponIndex = (_currentWeaponIndex + 1) % _weapons.Count;
        UpdateWeaponVisibility();
    }

    private void UpdateWeaponVisibility()
    {
        for (int i = 0; i < _weapons.Count; i++)
        {
            if (_weapons[i] != null)
            {
                _weapons[i].gameObject.SetActive(i == _currentWeaponIndex);
            }
        }
    }
}