
using UnityEngine;

[CreateAssetMenu(menuName = "BulletStrategy/Curve")]
public class BulletCurveMove : BulletMoveStrategyBase
{
    public float curveSpeed = 0f; // 회전 속도

    public override void BulletMovement(Bullet bullet)
    {
        // 가속
        bullet.CurrentSpeed += acceleration * Time.deltaTime;
        if (bullet.CurrentSpeed > maxSpeed)
        {
            bullet.CurrentSpeed = maxSpeed;
        }

        // 방향 회전
        if (curveSpeed != 0)
        {
            float curve = curveSpeed * Time.deltaTime;
            bullet.Direction = (Quaternion.Euler(0, 0, curve) * bullet.Direction).normalized;
        }

        // 이동
        Vector3 pos = bullet.BulletTransform.position;
        pos.x += bullet.Direction.x * bullet.CurrentSpeed * Time.deltaTime;
        pos.y += bullet.Direction.y * bullet.CurrentSpeed * Time.deltaTime;
        bullet.BulletTransform.position = pos;

        // 총알 회전 (시각적)
        float angle = Mathf.Atan2(bullet.Direction.y, bullet.Direction.x) * Mathf.Rad2Deg;
        bullet.BulletTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
