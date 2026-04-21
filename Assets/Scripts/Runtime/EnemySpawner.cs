using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<BattleUnit> enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    private List<BattleUnit> preparedEnemies = new List<BattleUnit>();
    private int nextIndex = 0;

    private const int MAX_SPAWN_COUNT = 3;

    public static event Action<BattleUnit> OnEnemySpawned;

    // Called ONLY by NextEncounter
    public List<BattleUnit> PrepareSpawn()
    {
        preparedEnemies.Clear();
        nextIndex = 0;

        int count = Random.Range(1, MAX_SPAWN_COUNT + 1);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            preparedEnemies.Add(enemyPrefabs[randomIndex]);
        }

        return preparedEnemies;
    }

    // Called ONLY by BattleManager
    public BattleUnit SpawnNext()
    {
        if (nextIndex >= preparedEnemies.Count)
            return null;

        Transform point = spawnPoints[nextIndex];
        BattleUnit prefab = preparedEnemies[nextIndex];

        BattleUnit enemy = Instantiate(prefab, point.position, point.rotation);

        OnEnemySpawned?.Invoke(enemy);

        nextIndex++;
        return enemy;
    }

    public List<BattleUnit> GetPreparedEnemies() => preparedEnemies;
}
