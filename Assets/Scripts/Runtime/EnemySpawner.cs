using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<BattleUnit> enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    private const int MAX_SPAWN_COUNT = 3;

    // Store ALL encounters here
    private List<List<BattleUnit>> preparedEncounters = new List<List<BattleUnit>>();

    // Tracks which enemy to spawn during battle
    private int nextIndex = 0;

    public static event Action<BattleUnit> OnEnemySpawned;

    // Called by NextEncounter for EACH encounter card
    public List<BattleUnit> PrepareSpawn()
    {
        Debug.Log("Preparing enemies for a new encounter...");

        List<BattleUnit> encounter = new List<BattleUnit>();

        int count = Random.Range(1, MAX_SPAWN_COUNT + 1);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            encounter.Add(enemyPrefabs[randomIndex]);
        }

        preparedEncounters.Add(encounter);
        return encounter;
    }

    // Called by BattleManager AFTER player selects an encounter
    public List<BattleUnit> GetPreparedEnemies(int index)
    {
        return preparedEncounters[index];
    }

    // Called by BattleManager to spawn enemies from the selected encounter
    public void BeginSpawningEncounter(int encounterIndex)
    {
        nextIndex = 0;
        currentEncounter = preparedEncounters[encounterIndex];
    }

    private List<BattleUnit> currentEncounter;

    public BattleUnit SpawnNext()
    {
        if (currentEncounter == null)
        {
            Debug.LogError("EnemySpawner: No encounter selected before spawning!");
            return null;
        }

        if (nextIndex >= currentEncounter.Count)
            return null;

        Transform point = spawnPoints[nextIndex];
        BattleUnit prefab = currentEncounter[nextIndex];

        BattleUnit enemy = Instantiate(prefab, point.position, point.rotation);
        OnEnemySpawned?.Invoke(enemy);

        nextIndex++;
        return enemy;
    }
}
