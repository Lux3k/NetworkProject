public class StopStrategy : IBulletStrategy
{
    public StopStrategy(PhaseData phase) { }

    public void OnStart(Bullet bullet)
    {
        bullet.CurrentSpeed = 0;
    }

    public void Move(Bullet bullet)
    {
        // Á¤Áö
    }
}