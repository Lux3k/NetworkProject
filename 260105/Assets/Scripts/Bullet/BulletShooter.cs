using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BulletShooter : MonoBehaviourPunCallbacks
{
    public void PlayPattern(BulletPaternSO pattern, Vector2 pos, Vector2 dir, BulletType bulletType)
    {
        if (pattern == null) return;

        // 네트워크로 패턴 ID만 전송
        if (photonView != null && photonView.IsMine)
        {
            photonView.RPC("RPC_PlayPattern", RpcTarget.All,
                pattern.patternID, pos, dir, (int)bulletType);
        }
    }

    [PunRPC]
    private void RPC_PlayPattern(int patternID, Vector2 pos, Vector2 dir, int bulletTypeInt)
    {
        BulletPaternSO pattern = GameManager.Instance.BulletManager.GetPattern(patternID);
        BulletType bulletType = (BulletType)bulletTypeInt;
        if (pattern == null)
        {
            Debug.LogError($"패턴 ID {patternID}를 가져오는 데 실패했습니다.");
            return;
        }
        if (pattern != null)
        {
            StartCoroutine(PatternRoutine(pattern, pos, dir, bulletType, photonView.ViewID));
        }
    }

    private IEnumerator PatternRoutine(BulletPaternSO pattern, Vector2 pos, Vector2 dir,
                                      BulletType bulletType, int ownerPhotonViewID)
    {
        // 그룹 생성 
        int groupID = GameManager.Instance.BulletManager.CreateGroup(pattern);

        float currentDuration = 0f;
        float currentSpinAngle = 0f;

        while (currentDuration < pattern.duration)
        {
            SpawnBullet(pattern, groupID, currentSpinAngle, dir, bulletType, ownerPhotonViewID);

            yield return new WaitForSeconds(pattern.fireInterval);
            currentDuration += pattern.fireInterval;
            currentSpinAngle += pattern.groupRotateAngle;
        }
    }

    private void SpawnBullet(BulletPaternSO pattern, int groupID, float spinAngle,
                        Vector2 baseDir, BulletType bulletType, int ownerPhotonViewID)
    {
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (pattern.angleRange / 2f) + spinAngle;

        float angleStep = pattern.bulletCount > 1
            ? pattern.angleRange / (pattern.bulletCount - 1)
            : 0f;

        if (pattern.angleRange >= 360f)
            angleStep = pattern.angleRange / pattern.bulletCount;

        // 첫 페이즈 전략 가져오기
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