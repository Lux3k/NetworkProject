using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PatternData
{
    public int patternID;
    public string name;
    public int bulletCount;
    public float fireInterval;
    public float duration;
    public float angleRange;
    public float groupRotateAngle;
    public Color bulletColor;
    public List<PhaseData> phases;
}