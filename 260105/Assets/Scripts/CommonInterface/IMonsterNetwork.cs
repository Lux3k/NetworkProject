using UnityEngine;

public interface IMonsterSpawnNetwork
{
    void RequestSpawn(int monsterID, Vector2 pos, int uniqueID);
}

public interface IMonsterCombatNetwork
{
    void OnMonsterAttack(int uniqueID, int patternID, Vector2 targetPos);
    void OnMonsterDamaged(int uniqueID, int damage);
    void OnMonsterDied(int uniqueID);
}