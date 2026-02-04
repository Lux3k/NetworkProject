using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private float spawnRadius = 15f;

    private IMonsterSpawnNetwork _network;
    private Stack<MonsterController> _pool = new();
    private int _nextUniqueID = 0;

    private MonsterController _currentBoss;

    public int ActiveCount => poolSize - _pool.Count;
    public bool IsBossAlive => _currentBoss != null && _currentBoss.gameObject.activeSelf;

    public void Initialize(IMonsterSpawnNetwork network)
    {
        _network = network;
    }

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var monster = Instantiate(monsterPrefab);
            monster.SetActive(false);
            _pool.Push(monster.GetComponent<MonsterController>());
        }
    }

    public IEnumerator RunWave(WaveData wave)
    {
        bool hasBoss = wave.bossID > 0;
        _currentBoss = null;

        if (hasBoss && PhotonNetwork.IsMasterClient)
        {
            Vector2 bossPos = GetRandomSpawnPosition();
            int uniqueID = _nextUniqueID++;
            _network.RequestSpawn(wave.bossID, bossPos, uniqueID);
        }

        if (hasBoss && wave.duration <= 0f)
        {
            if (wave.keepSpawn)
                yield return StartCoroutine(SpawnUntilBossDead(wave));
            else
                yield return new WaitUntil(() => !IsBossAlive);
        }
        else if (hasBoss && wave.duration > 0f)
        {
            if (wave.keepSpawn)
                yield return StartCoroutine(SpawnForDurationOrBossDead(wave));
            else
            {
                float elapsed = 0f;
                while (elapsed < wave.duration && IsBossAlive)
                {
                    yield return new WaitForSeconds(1f);
                    elapsed += 1f;
                }
            }
        }
        else
        {
            yield return StartCoroutine(SpawnForDuration(wave));
        }
    }

    private IEnumerator SpawnForDuration(WaveData wave)
    {
        float elapsed = 0f;
        while (elapsed < wave.duration)
        {
            TrySpawnMinion(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
            elapsed += wave.spawnInterval;
        }
    }

    private IEnumerator SpawnUntilBossDead(WaveData wave)
    {
        while (IsBossAlive)
        {
            TrySpawnMinion(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    private IEnumerator SpawnForDurationOrBossDead(WaveData wave)
    {
        float elapsed = 0f;
        while (elapsed < wave.duration && IsBossAlive)
        {
            TrySpawnMinion(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
            elapsed += wave.spawnInterval;
        }
    }

    private void TrySpawnMinion(WaveData wave)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (ActiveCount >= wave.maxAlive) return;
        if (_pool.Count <= 0) return;

        int monsterID = wave.monsterIDs[Random.Range(0, wave.monsterIDs.Length)];
        Vector2 spawnPos = GetRandomSpawnPosition();
        int uniqueID = _nextUniqueID++;
        _network.RequestSpawn(monsterID, spawnPos, uniqueID);
    }

    public MonsterController Spawn(int monsterID, Vector2 pos, int uniqueID)
    {
        MonsterController monster;
        if (_pool.Count > 0)
            monster = _pool.Pop();
        else
            monster = Instantiate(monsterPrefab).GetComponent<MonsterController>();

        monster.transform.position = pos;
        monster.gameObject.SetActive(true);
        return monster;
    }

    public void SetBoss(MonsterController boss)
    {
        _currentBoss = boss;
    }

    public void Return(MonsterController monster)
    {
        if (monster == _currentBoss)
            _currentBoss = null;

        monster.gameObject.SetActive(false);
        _pool.Push(monster);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return Random.insideUnitCircle * spawnRadius;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 camPos = cam.transform.position;
        return camPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
    }
}