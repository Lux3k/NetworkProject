using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class DataManager : SingletonBehaviour<DataManager>
{

    [Header("Google Sheets URLs")]
    [SerializeField] private string patternSheetURL;
    [SerializeField] private string phaseSheetURL;
    [SerializeField] private string weaponSheetURL;

    [Header("로컬 CSV Fallback")]
    [SerializeField] private string patternCSVPath = "Data/patterns";
    [SerializeField] private string phaseCSVPath = "Data/phases";
    [SerializeField] private string weaponCSVPath = "Data/weapons";

    private Dictionary<int, PatternData> _patterns = new();
    private Dictionary<int, PhaseData> _phases = new();
    private Dictionary<int, WeaponData> _weapons = new();
    private Dictionary<int, IBulletStrategy> _strategyCache = new();

    public bool IsLoaded { get; private set; }

    void Awake()
    {
        Init();
    }

    protected override void Init()
    {

        base.isDestroyOnLoad = true;

        base.Init();
    }
    IEnumerator Start()
    {
        yield return StartCoroutine(LoadCSV(phaseSheetURL, phaseCSVPath, ParsePhases));
        yield return StartCoroutine(LoadCSV(patternSheetURL, patternCSVPath, ParsePatterns));
        yield return StartCoroutine(LoadCSV(weaponSheetURL, weaponCSVPath, ParseWeapons));

        IsLoaded = true;
        Debug.Log($"DataManager 로드 완료 - 패턴:{_patterns.Count} 페이즈:{_phases.Count} 무기:{_weapons.Count}");
    }

    IEnumerator LoadCSV(string sheetURL, string localPath, Action<string> parser)
    {
        // 네트워크 우선
        if (!string.IsNullOrEmpty(sheetURL))
        {
            using var request = UnityWebRequest.Get(sheetURL);
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"시트에서 로드 성공: {sheetURL}");
                parser(request.downloadHandler.text);
                yield break;
            }

            Debug.LogWarning($"시트 로드 실패, 로컬 fallback: {request.error}");
        }

        // 로컬 fallback
        var csv = Resources.Load<TextAsset>(localPath);
        if (csv != null)
        {
            Debug.Log($"로컬 CSV 로드: {localPath}");
            parser(csv.text);
        }
        else
        {
            Debug.LogError($"로컬 CSV도 없음: {localPath}");
        }
    }

    void ParsePhases(string csvText)
    {
        var lines = csvText.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            var phase = new PhaseData
            {
                PhaseID = int.Parse(cols[0].Trim()),
                strategyType = Enum.Parse<StrategyType>(cols[1].Trim()),
                startSpeed = float.Parse(cols[2].Trim()),
                maxSpeed = float.Parse(cols[3].Trim()),
                acceleration = float.Parse(cols[4].Trim()),
                duration = float.Parse(cols[5].Trim()),
                param1 = float.Parse(cols[6].Trim()),
                param2 = float.Parse(cols[7].Trim()),
                param3 = float.Parse(cols[8].Trim())
            };
            _phases[phase.PhaseID] = phase;
        }
    }

    void ParsePatterns(string csvText)
    {
        var lines = csvText.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            var pattern = new PatternData
            {
                patternID = int.Parse(cols[0].Trim()),
                name = cols[1].Trim(),
                bulletCount = int.Parse(cols[2].Trim()),
                fireInterval = float.Parse(cols[3].Trim()),
                duration = float.Parse(cols[4].Trim()),
                angleRange = float.Parse(cols[5].Trim()),
                groupRotateAngle = float.Parse(cols[6].Trim()),
                bulletColor = ParseColor(cols[7].Trim()),
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

    void ParseWeapons(string csvText)
    {
        var lines = csvText.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            var weapon = new WeaponData
            {
                weaponID = int.Parse(cols[0].Trim()),
                name = cols[1].Trim(),
                patternID = int.Parse(cols[2].Trim()),
                damage = int.Parse(cols[3].Trim()),
                attackSpeed = float.Parse(cols[4].Trim()),
            };
            _weapons[weapon.weaponID] = weapon;
        }
    }

    Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;
        return Color.white;
    }

    public PatternData GetPattern(int id)
    {
        _patterns.TryGetValue(id, out var pattern);
        return pattern;
    }

    public WeaponData GetWeapon(int id)
    {
        _weapons.TryGetValue(id, out var weapon);
        return weapon;
    }

    public IBulletStrategy GetStrategy(PhaseData phase)
    {
        if (_strategyCache.TryGetValue(phase.PhaseID, out var cached))
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

        _strategyCache[phase.PhaseID] = strategy;
        return strategy;
    }

    public void ClearStrategyCache()
    {
        _strategyCache.Clear();
    }
}
