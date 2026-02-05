using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private List<Weapon> _weapons = new();
    private int _currentIndex = 0;

    public Weapon CurrentWeapon => _weapons.Count > 0 ? _weapons[_currentIndex] : null;

    void Update()
    {
        foreach (var weapon in _weapons)
            weapon.UpdateTimer();

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0f)
            SwitchWeapon(_currentIndex + 1);
        else if (scroll < 0f)
            SwitchWeapon(_currentIndex - 1);
    }

    public void Fire(Vector2 direction, BulletType bulletType)
    {
        CurrentWeapon?.Fire(direction, bulletType);
    }

    private void SwitchWeapon(int newIndex)
    {
        if (_weapons.Count == 0) return;

        CurrentWeapon.gameObject.SetActive(false);

        if (newIndex >= _weapons.Count)
            newIndex = 0;
        else if (newIndex < 0)
            newIndex = _weapons.Count - 1;

        _currentIndex = newIndex;

        CurrentWeapon.gameObject.SetActive(true);
    }

    public void AddWeapon(Weapon weapon)
    {
        _weapons.Add(weapon);
    }
}
