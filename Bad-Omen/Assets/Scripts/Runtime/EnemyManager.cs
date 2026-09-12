using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private List<BattleUnit> enemies = new List<BattleUnit>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        EnemySpawner.OnEnemySpawned += HandleEnemySpawned;
    }
    public void UnregisterEnemy(BattleUnit enemy)
    {
        enemies.Remove(enemy);
    }

    void OnDisable()
    {
        EnemySpawner.OnEnemySpawned -= HandleEnemySpawned;
    }

    private void HandleEnemySpawned(BattleUnit enemy)
    {
        RegisterEnemy(enemy);
    }

    public void RegisterEnemy(BattleUnit enemy)
    {
        enemies.Add(enemy);
        Debug.Log($"Enemy registered: {enemy.name}");
    }

    public List<BattleUnit> GetEnemies()
    {
        return enemies;
    }


}
