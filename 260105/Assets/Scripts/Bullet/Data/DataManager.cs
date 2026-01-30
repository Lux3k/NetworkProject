using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [SerializeField] private string patternCSVPath = "Data/patterns";
    [SerializeField] private string phaseCSVPath = "Data/phases";

    private Dictionary<int, PatternData> _patterns = new();
    private Dictionary<int, PhaseData> _phases = new();
    private Dictionary<int, IBulletStrategy> _strategyCache = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadPhases();
        LoadPatterns();
    }


    void LoadPhases()
    {
        var csv = Resources.Load<TextAsset>(phaseCSVPath);

        var lines = csv.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            var phase = new PhaseData
            {
                PhaseID = int.Parse(cols[0]),
                strategyType = System.Enum.Parse<StrategyType>(cols[1]),
                startSpeed = float.Parse(cols[2]),
                maxSpeed = float.Parse(cols[3]),
                acceleration = float.Parse(cols[4]),
                duration = float.Parse(cols[5]),
                param1 = float.Parse(cols[6]),
                param2 = float.Parse(cols[7]),
                param3 = float.Parse(cols[8])
            };
            _phases[phase.PhaseID] = phase;
        }
    }

    void LoadPatterns()
    {
        var csv = Resources.Load<TextAsset>(patternCSVPath);

        var lines = csv.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            var pattern = new PatternData
            {
                patternID = int.Parse(cols[0]),
                name = cols[1],
                bulletCount = int.Parse(cols[2]),
                fireInterval = float.Parse(cols[3]),
                duration = float.Parse(cols[4]),
                angleRange = float.Parse(cols[5]),
                groupRotateAngle = float.Parse(cols[6]),
                bulletColor = ParseColor(cols[7]),
                phases = new List<PhaseData>()
            };

            var phaseIDs = cols[8].Split('|');
            foreach (var id in phaseIDs)
            {
                int phaseID = int.Parse(id.Trim());
                if (_phases.TryGetValue(phaseID, out var phase))
                    pattern.phases.Add(phase);
            }

            _patterns[pattern.patternID] = pattern;
        }

    }

    Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex.Trim(), out Color color))
            return color;
        return Color.white;
    }

    public PatternData GetPattern(int id)
    {
        if (_patterns.TryGetValue(id, out var pattern))
            return pattern;
        return null;
    }

    public IBulletStrategy GetStrategy(PhaseData phase)
    {
        if (_strategyCache.TryGetValue(phase.PhaseID, out var cached))
            return cached;

        IBulletStrategy strategy = phase.strategyType switch
        {
            StrategyType.Straight => new StraightStrategy(phase),
            StrategyType.Curve => new CurveStrategy(phase),
            StrategyType.Stop => new StopStrategy(phase),
            _ => new StraightStrategy(phase)
        };

        _strategyCache[phase.PhaseID] = strategy;
        return strategy;
    }


    // 스테이지 전환 시
    public void ClearStrategyCache()
    {
        _strategyCache.Clear();
    }
}
