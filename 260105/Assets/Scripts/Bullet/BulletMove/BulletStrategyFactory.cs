using System.Collections.Generic;

public class StrategyFactory
{
    private  Dictionary<int, IBulletStrategy> _cache = new();

    public IBulletStrategy Create(PhaseData phase)
    {
        int key = phase.PhaseID;

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        IBulletStrategy strategy;
        switch (phase.strategyType)
        {
            case StrategyType.Straight:
                strategy = new StraightStrategy(phase);
                break;
            case StrategyType.Curve:
                strategy = new CurveStrategy(phase);
                break;
            case StrategyType.Stop:
                strategy = new StopStrategy(phase);
                break;
            default:
                strategy = new StraightStrategy(phase);
                break;
        }

        _cache[key] = strategy;
        return strategy;
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}