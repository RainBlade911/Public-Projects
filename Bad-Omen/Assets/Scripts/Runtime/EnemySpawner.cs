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

    private const int ENCOUNTER_COUNT = 3;

    // Store ALL encounters here
    private List<List<BattleUnit>> preparedEncounters = new List<List<BattleUnit>>();
    private List<bool> preparedEncounterIsBoss = new List<bool>();

    private List<BattleUnit> currentEncounter;
    private bool currentEncounterIsBoss = false;
    private int nextIndex = 0;

    public static event Action<BattleUnit> OnEnemySpawned;

    private bool ValidEncounter = false;

    // Called by NextEncounter for EACH encounter card
    //public List<BattleUnit> PrepareSpawn()
    //{
    //    Debug.Log("Preparing enemies for a new encounter...");

    //    List<BattleUnit> encounter = new List<BattleUnit>();

    //    int count = Random.Range(1, MAX_SPAWN_COUNT + 1);

    //    for (int i = 0; i < count; i++)
    //    {
    //        int randomIndex = Random.Range(0, enemyPrefabs.Count);
    //        encounter.Add(enemyPrefabs[randomIndex]);
    //    }

    //    preparedEncounters.Add(encounter);
    //    return encounter;
    //}

    public List<BattleUnit> PrepareSpawn(int tier)
    {
        Debug.Log($"Preparing encounter for tier {tier}...");

        int lowerBound = tier * 7 + 1;
        int upperBound = lowerBound + 6;

        const int MAX_ATTEMPTS = 500;
        List<BattleUnit> lastEncounter = null;
        int lastDifficulty = 0;

        //bool spawnBoss = bossPrefab != null && Random.value <= bossEncounterChance;

        //if (spawnBoss)
        //{
        //    encounter.Add(bossPrefab);
        //    preparedEncounters.Add(encounter);
        //    preparedEncounterIsBoss.Add(true);

        //    Debug.Log("Prepared boss encounter.");
        //    return encounter;
        //}

        //int count = Random.Range(1, MAX_SPAWN_COUNT + 1);

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            List<BattleUnit> encounter = new List<BattleUnit>();
            int difficultySum = 0;

            int count = Random.Range(1, MAX_SPAWN_COUNT + 1);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, enemyPrefabs.Count);
                BattleUnit enemy = enemyPrefabs[randomIndex];

                encounter.Add(enemy);
                difficultySum += enemy.GetEnemy().GetEnemyData().GetDifficulty();
            }

            lastEncounter = encounter;
            lastDifficulty = difficultySum;

            if (difficultySum >= lowerBound && difficultySum <= upperBound)
            {
                preparedEncounters.Add(encounter);
                return encounter;
            }
        }

        Debug.LogWarning(
            $"EnemySpawner: Could not find encounter in [{lowerBound}, {upperBound}] after {MAX_ATTEMPTS} attempts. " +
            $"Using closest difficulty {lastDifficulty} instead."
        );

        preparedEncounters.Add(lastEncounter);
        return lastEncounter;
    }

    public List<BattleUnit> PrepareBossEncounter()
    {
        List<BattleUnit> encounter = new List<BattleUnit>();
        encounter.Add(bossPrefab);

        preparedEncounters.Add(encounter);
        preparedEncounterIsBoss.Add(true);

        Debug.Log("Prepared boss encounter.");
        return encounter;
    }


    public BattleUnit SpawnBoss()
    {
        return bossPrefab;
    }




    //    preparedEncounters.Add(encounter);
    //    //preparedEncounterIsBoss.Add(false);

    //    return encounter;
    //}


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
            enemy.transform.localScale *= 1f;
        }

        OnEnemySpawned?.Invoke(enemy);

        nextIndex++;
        return enemy;
    }
}