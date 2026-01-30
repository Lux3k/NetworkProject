using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int weaponID;
    [SerializeField] private Transform firePoint;
    private BulletShooter shooter;

    private WeaponData _data;
    private float _attackTimer;

    public int WeaponID => weaponID;
    public string WeaponName => _data != null ? _data.name : "Unknown";
    public bool CanFire => _attackTimer <= 0f;

    void Start()
    {
        _data = DataManager.Instance.GetWeapon(weaponID);
        shooter = GetComponentInParent<BulletShooter>();
    }

    public void UpdateTimer()
    {
        if (_attackTimer > 0f)
            _attackTimer -= Time.deltaTime;
    }

    public void Fire(Vector2 dir, BulletType bulletType)
    {
        if (!CanFire || _data == null) return;

        Vector2 pos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;

        shooter.PlayPattern(_data.patternID, pos, dir, bulletType);

        _attackTimer = 1f / _data.attackSpeed;
    }
}