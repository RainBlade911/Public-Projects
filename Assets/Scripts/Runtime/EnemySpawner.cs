using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static event Action<BattleUnit> OnEnemySpawned;

    [SerializeField] private BattleUnit enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private int nextIndex = 0;

    public BattleUnit Spawn()
    {
        if (nextIndex >= spawnPoints.Length)
            return null;

        Transform point = spawnPoints[nextIndex];
        nextIndex++;

        BattleUnit enemy = Instantiate(enemyPrefab, point.position, point.rotation);
        OnEnemySpawned?.Invoke(enemy);
        return enemy;
    }


}
