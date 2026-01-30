using UnityEngine;

public class CurveStrategy : IBulletStrategy
{
    private float _startSpeed;
    private float _maxSpeed;
    private float _acceleration;
    private float _curveAngle;

    public CurveStrategy(PhaseData phase)
    {
        _startSpeed = phase.startSpeed;
        _maxSpeed = phase.maxSpeed;
        _acceleration = phase.acceleration;
        _curveAngle = phase.param1;
    }

    public void OnStart(Bullet bullet)
    {
        bullet.CurrentSpeed = _startSpeed;
    }

    public void Move(Bullet bullet)
    {
        float angle = Mathf.Atan2(bullet.Direction.y, bullet.Direction.x) * Mathf.Rad2Deg;
        angle += _curveAngle * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;
        bullet.Direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        bullet.CurrentSpeed += _acceleration * Time.deltaTime;
        if (bullet.CurrentSpeed > _maxSpeed)
            bullet.CurrentSpeed = _maxSpeed;

        Vector3 pos = bullet.BulletTransform.position;
        pos.x += bullet.Direction.x * bullet.CurrentSpeed * Time.deltaTime;
        pos.y += bullet.Direction.y * bullet.CurrentSpeed * Time.deltaTime;
        bullet.BulletTransform.position = pos;

        bullet.BulletTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
}