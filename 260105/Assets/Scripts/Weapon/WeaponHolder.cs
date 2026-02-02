using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private List<Weapon> weapons = new();
    private int _currentIndex = 0;

    public Weapon CurrentWeapon => weapons.Count > 0 ? weapons[_currentIndex] : null;

    void Update()
    {
        foreach (var weapon in weapons)
            weapon.UpdateTimer();

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0f)
            SwitchWeapon(_currentIndex + 1);
        else if (scroll < 0f)
            SwitchWeapon(_currentIndex - 1);
    }

    public void Fire(Vector2 dir, BulletType bulletType)
    {
        CurrentWeapon?.Fire(dir, bulletType);
    }

    private void SwitchWeapon(int newIndex)
    {
        if (weapons.Count == 0) return;

        CurrentWeapon.gameObject.SetActive(false);

        if (newIndex >= weapons.Count)
            newIndex = 0;
        else if (newIndex < 0)
            newIndex = weapons.Count - 1;

        _currentIndex = newIndex;

        CurrentWeapon.gameObject.SetActive(true);
    }

    public void AddWeapon(Weapon weapon)
    {
        weapons.Add(weapon);
    }
}
