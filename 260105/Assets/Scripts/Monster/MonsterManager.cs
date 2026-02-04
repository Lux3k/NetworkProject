using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviourPunCallbacks, IMonsterSpawnNetwork, IMonsterCombatNetwork
{
    [SerializeField] private MonsterSpawner _spawner;
    private Dictionary<int, MonsterController> _activeMonsters = new();
    private int _nextMonsterID = 0;
    void Start()
    {
        _spawner.Initialize(this);
    }
    public void RequestSpawn(int monsterID, Vector2 pos, int uniqueID)
    {
        photonView.RPC("RPC_SpawnMonster", RpcTarget.All, monsterID, pos, uniqueID);
    }

    [PunRPC]
    void RPC_SpawnMonster(int monsterID, Vector2 pos, int uniqueID)
    {

        var monster = _spawner.Spawn(monsterID, pos, uniqueID);
        monster.Initialize(monsterID, uniqueID, this);
        monster.gameObject.SetActive(true);
        _activeMonsters[uniqueID] = monster;

        if (monsterID >= 3000)
            _spawner.SetBoss(monster);
    }

    public void OnMonsterDamaged(int uniqueID, int damage)
    {
        photonView.RPC("RPC_MonsterDamaged", RpcTarget.All, uniqueID, damage);
    }

    [PunRPC]
    void RPC_MonsterDamaged(int uniqueID, int damage)
    {
        if (_activeMonsters.TryGetValue(uniqueID, out var monster))
            monster.ApplyDamage(damage);
    }

    public void OnMonsterDied(int uniqueID)
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_MonsterDied", RpcTarget.All, uniqueID);
    }

    [PunRPC]
    void RPC_MonsterDied(int uniqueID)
    {
        if (_activeMonsters.TryGetValue(uniqueID, out var monster))
        {
            _activeMonsters.Remove(uniqueID);
            _spawner.Return(monster);
        }
    }

    public void OnMonsterAttack(int uniqueID, int patternID, Vector2 targetPos)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_MonsterAttack", RpcTarget.All, uniqueID, patternID, targetPos);
        }
    }

    [PunRPC]
    void RPC_MonsterAttack(int uniqueID, int patternID, Vector2 targetPos)
    {
        if (_activeMonsters.TryGetValue(uniqueID, out var monster))
        {
            monster.ExecuteAttack(patternID, targetPos);
        }
    }

}