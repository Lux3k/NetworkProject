using UnityEngine;

public abstract class BulletMoveStrategyBase : ScriptableObject
{
    public float startSpeed = 5f;
    public float acceleration = 0f;
    public float maxSpeed = 10f;

    // 전략 시작 시 호출
    public virtual void OnStrategyStart(Bullet bullet) { }

    // 매 프레임 호출
    public abstract void BulletMovement(Bullet bullet);
}

