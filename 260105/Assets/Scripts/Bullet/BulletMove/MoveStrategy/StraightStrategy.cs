using UnityEngine;

public class StraightStrategy : IBulletStrategy
{
    private float _startSpeed;
    private float _maxSpeed;
    private float _acceleration;

    public StraightStrategy(PhaseData phase)
    {
        _startSpeed = phase.startSpeed;
        _maxSpeed = phase.maxSpeed;
        _acceleration = phase.acceleration;
    }

    public void OnStart(Bullet bullet)
    {
        bullet.CurrentSpeed = _startSpeed;
    }

    public void Move(Bullet bullet)
    {
        bullet.CurrentSpeed += _acceleration * Time.deltaTime;
        if (bullet.CurrentSpeed > _maxSpeed)
            bullet.CurrentSpeed = _maxSpeed;

        Vector3 pos = bullet.BulletTransform.position;
        pos.x += bullet.Direction.x * bullet.CurrentSpeed * Time.deltaTime;
        pos.y += bullet.Direction.y * bullet.CurrentSpeed * Time.deltaTime;
        bullet.BulletTransform.position = pos;
    }
}