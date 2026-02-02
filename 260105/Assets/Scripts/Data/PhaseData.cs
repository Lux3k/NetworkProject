
public enum StrategyType
{
    None,
    Straight,
    Curve,
    Stop
}

[System.Serializable]
public class PhaseData
{
    public int PhaseID;
    public StrategyType strategyType;
    public float startSpeed;
    public float maxSpeed;
    public float acceleration;
    public float duration;
    public float param1; 
    public float param2;
    public float param3;
}