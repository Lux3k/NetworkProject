using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BulletShooter : MonoBehaviourPunCallbacks
{
    [Header("테스트용 SO")]
    [SerializeField] private BulletPatternSO testPattern;

    public void PlayPattern(int patternID, Vector2 pos, Vector2 dir, BulletType bulletType)
    {
        if (photonView != null && photonView.IsMine)
            photonView.RPC("RPC_PlayPattern", RpcTarget.All,
                patternID, pos, dir, (int)bulletType);
    }

    [PunRPC]
    private void RPC_PlayPattern(int patternID, Vector2 pos, Vector2 dir, int bulletTypeInt)
    {
        BulletType bulletType = (BulletType)bulletTypeInt;

        if (testPattern != null && testPattern.patternID == patternID)
        {
            StartCoroutine(PatternRoutine(testPattern, pos, dir, bulletType, PhotonNetwork.LocalPlayer.ActorNumber));
            return;
        }

        PatternData csvPattern = DataManager.Instance.GetPattern(patternID);
        if (csvPattern != null)
        {
            StartCoroutine(PatternRoutine(csvPattern, pos, dir, bulletType, PhotonNetwork.LocalPlayer.ActorNumber));
            return;
        }
    }

    private IEnumerator PatternRoutine(PatternData pattern, Vector2 pos, Vector2 dir,
                                  BulletType bulletType, int ownerPhotonViewID)
    {
        float currentDuration = 0f;
        float currentSpinAngle = 0f;

        while (currentDuration < pattern.duration)
        {
            // 매 발사마다 그룹 생성
            int groupID = GameManager.Instance.BulletManager.CreateGroup(pattern);
            SpawnBullet(pattern, groupID, currentSpinAngle, dir, bulletType, ownerPhotonViewID);

            yield return new WaitForSeconds(pattern.fireInterval);
            currentDuration += pattern.fireInterval;
            currentSpinAngle += pattern.groupRotateAngle;
        }
    }
    private void SpawnBullet(PatternData pattern, int groupID, float spinAngle,
                            Vector2 baseDir, BulletType bulletType, int ownerPhotonViewID)
    {
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (pattern.angleRange / 2f) + spinAngle;

        float angleStep = pattern.bulletCount > 1
            ? pattern.angleRange / (pattern.bulletCount - 1)
            : 0f;

        if (pattern.angleRange >= 360f)
            angleStep = pattern.angleRange / pattern.bulletCount;

        IBulletStrategy initialStrategy = null;
        if (pattern.phases != null && pattern.phases.Count > 0)
        {
            initialStrategy = DataManager.Instance.GetStrategy(pattern.phases[0]);
        }

        for (int i = 0; i < pattern.bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 dir = DegreeToVector2(currentAngle);

            GameManager.Instance.BulletManager.GetBullet(
                transform.position, dir, bulletType,
                pattern.bulletColor, groupID, ownerPhotonViewID,initialStrategy);
        }
    }

    private IEnumerator PatternRoutine(BulletPatternSO pattern, Vector2 pos, Vector2 dir,
                                    BulletType bulletType, int ownerPhotonViewID)
    {
        float currentDuration = 0f;
        float currentSpinAngle = 0f;

        while (currentDuration < pattern.duration)
        {
            int groupID = GameManager.Instance.BulletManager.CreateGroup(pattern);
            SpawnBullet(pattern, groupID, currentSpinAngle, dir, bulletType, ownerPhotonViewID);

            yield return new WaitForSeconds(pattern.fireInterval);
            currentDuration += pattern.fireInterval;
            currentSpinAngle += pattern.groupRotateAngle;
        }
    }
    private void SpawnBullet(BulletPatternSO pattern, int groupID, float spinAngle,
                        Vector2 baseDir, BulletType bulletType, int ownerPhotonViewID)
    {
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (pattern.angleRange / 2f) + spinAngle;

        float angleStep = pattern.bulletCount > 1
            ? pattern.angleRange / (pattern.bulletCount - 1)
            : 0f;

        if (pattern.angleRange >= 360f)
            angleStep = pattern.angleRange / pattern.bulletCount;

        BulletMoveStrategyBase initialStrategy = null;
        if (pattern.phases != null && pattern.phases.Length > 0)
        {
            initialStrategy = pattern.phases[0].strategy;
        }

        for (int i = 0; i < pattern.bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 dir = DegreeToVector2(currentAngle);

            
            GameManager.Instance.BulletManager.GetBullet(
                transform.position,
                dir,
                bulletType,
                pattern.bulletColor,  
                groupID,
                ownerPhotonViewID,
                initialStrategy);     
        }
    }

    private Vector2 DegreeToVector2(float degree)
    {
        float radian = degree * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
}