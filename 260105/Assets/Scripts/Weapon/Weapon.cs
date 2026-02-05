using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int _weaponID;
    [SerializeField] private Transform _firePoint;
    private BulletShooter _shooter;

    private WeaponData _data;
    private float _attackTimer;

    public int WeaponID => _weaponID;
    public string WeaponName => _data != null ? _data.name : "Unknown";
    public bool CanFire => _attackTimer <= 0f;

    void Start()
    {
        _data = DataManager.Instance.GetWeapon(_weaponID);
        _shooter = GetComponentInParent<BulletShooter>();
    }

    public void UpdateTimer()
    {
        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;
    }

    public void Fire(Vector2 direction, BulletType bulletType)
    {
        if (!CanFire || _data == null) return;

        Vector2 pos = _firePoint != null ? (Vector2)_firePoint.position : (Vector2)transform.position;

        _shooter.PlayPattern(_data.patternID, pos, direction, bulletType);

        _attackTimer = 1f / _data.attackSpeed;
    }
}