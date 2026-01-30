using UnityEngine;

// 직선 이동 전략
[CreateAssetMenu(menuName = "BulletStrategy/Straight")]
public class StraightMoveStrategy : BulletMoveStrategyBase
{
    public override void BulletMovement(Bullet bullet)
    {
        // 가속
        bullet.CurrentSpeed += acceleration * Time.deltaTime;
        if (bullet.CurrentSpeed > maxSpeed)
        {
            bullet.CurrentSpeed = maxSpeed;
        }

        // 이동
        Vector3 pos = bullet.BulletTransform.position;
        pos.x += bullet.Direction.x * bullet.CurrentSpeed * Time.deltaTime;
        pos.y += bullet.Direction.y * bullet.CurrentSpeed * Time.deltaTime;
        bullet.BulletTransform.position = pos;
    }
}
