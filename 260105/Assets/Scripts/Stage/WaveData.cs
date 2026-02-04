[System.Serializable]
public class WaveData
{
    public int waveID;
    public int[] monsterIDs;    // 잡몹 랜덤 스폰 풀
    public float spawnInterval; // 잡몹 스폰 주기(초)
    public float duration;      // 웨이브 지속 시간(초). 0이면 보스 처치가 종료 조건
    public int maxAlive;        // 잡몹 동시 생존 최대 수
    public int bossID;          // 0이면 보스 없음, 값 있으면 보스 소환
    public bool keepSpawn;      // true면 보스 나와도 잡몹 계속 스폰
}