
using UnityEngine;

[CreateAssetMenu(menuName = "BulletStrategy/ColorChange")]
public class BulletColorChange : BulletMoveStrategyBase
{
    public Color bulletColor = Color.white;

    public override void OnStrategyStart(Bullet bullet)
    {
        bullet.ColorChange(bulletColor);
    }

    public override void BulletMovement(Bullet bullet)
    {
    }
}