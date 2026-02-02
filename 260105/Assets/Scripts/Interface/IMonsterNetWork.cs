using UnityEngine;

public interface IMonsterSpawnNetwork
{
    void RequestSpawn(int monsterID, Vector2 pos, int uniqueID);
}

public interface IMonsterCombatNetwork
{
    void OnMonsterDamaged(int uniqueID, int damage);
    void OnMonsterDied(int uniqueID);
}