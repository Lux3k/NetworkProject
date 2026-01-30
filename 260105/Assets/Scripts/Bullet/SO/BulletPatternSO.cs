using UnityEngine;

[System.Serializable]
public class BulletPhase
{
    public BulletMoveStrategyBase strategy;
    public float duration;
}

[CreateAssetMenu(fileName = "NewPattern", menuName = "BulletPattern")]
public class BulletPatternSO : ScriptableObject
{
    [Header("Network")]
    public int patternID; //  네트워크 전송용 ID

    [Header("Visual")]
    public Sprite bulletSprite;
    public Color bulletColor = Color.white;

    [Header("Pattern")]
    public int bulletCount = 12;
    public float fireInterval = 0.1f;
    public float duration = 5.0f;
    public float angleRange = 360f;
    public float groupRotateAngle = 0f;

    [Header("Movement")]
    public BulletPhase[] phases;
}