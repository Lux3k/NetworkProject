
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private int[] monsterIDs;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private int maxMonsters = 20;

    private IMonsterSpawnNetwork _network;
    private Stack<MonsterController> _pool = new();
    private float _spawnTimer;
    private int _nextUniqueID = 0;

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

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= spawnInterval && _pool.Count > 0)
        {
            _spawnTimer = 0f;
            int monsterID = monsterIDs[Random.Range(0, monsterIDs.Length)];
            Vector2 spawnPos = GetRandomSpawnPosition();
            int uniqueID = _nextUniqueID++;

            _network.RequestSpawn(monsterID, spawnPos, uniqueID);
        }
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

    public void Return(MonsterController monster)
    {
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
