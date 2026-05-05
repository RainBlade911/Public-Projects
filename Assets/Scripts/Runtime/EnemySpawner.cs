using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Normal Enemy Prefabs")]
    [SerializeField] private List<BattleUnit> enemyPrefabs;

    [Header("Boss Prefab")]
    [SerializeField] private BattleUnit bossPrefab;
    [SerializeField] private float bossEncounterChance = 0.15f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private const int MAX_SPAWN_COUNT = 3;

    private List<List<BattleUnit>> preparedEncounters = new List<List<BattleUnit>>();
    private List<bool> preparedEncounterIsBoss = new List<bool>();

    private List<BattleUnit> currentEncounter;
    private bool currentEncounterIsBoss = false;
    private int nextIndex = 0;

    public static event Action<BattleUnit> OnEnemySpawned;

    public List<BattleUnit> PrepareSpawn()
    {
        Debug.Log("Preparing enemies for a new encounter...");

        List<BattleUnit> encounter = new List<BattleUnit>();

        bool spawnBoss = bossPrefab != null && Random.value <= bossEncounterChance;

        if (spawnBoss)
        {
            encounter.Add(bossPrefab);
            preparedEncounters.Add(encounter);
            preparedEncounterIsBoss.Add(true);

            Debug.Log("Prepared boss encounter.");
            return encounter;
        }

        int count = Random.Range(1, MAX_SPAWN_COUNT + 1);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            encounter.Add(enemyPrefabs[randomIndex]);
        }

        preparedEncounters.Add(encounter);
        preparedEncounterIsBoss.Add(false);

        return encounter;
    }

    public List<BattleUnit> GetPreparedEnemies(int index)
    {
        return preparedEncounters[index];
    }

    public bool IsPreparedEncounterBoss(int index)
    {
        if (index < 0 || index >= preparedEncounterIsBoss.Count)
            return false;

        return preparedEncounterIsBoss[index];
    }

    public bool IsCurrentEncounterBoss()
    {
        return currentEncounterIsBoss;
    }

    public void BeginSpawningEncounter(int encounterIndex)
    {
        nextIndex = 0;
        currentEncounter = preparedEncounters[encounterIndex];
        currentEncounterIsBoss = IsPreparedEncounterBoss(encounterIndex);
    }

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

        if (currentEncounterIsBoss)
        {
            enemy.transform.localScale *= 1.5f;
        }

        OnEnemySpawned?.Invoke(enemy);

        nextIndex++;
        return enemy;
    }
}