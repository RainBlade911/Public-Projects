using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private string deathSceneName = "DeathScene";
    [SerializeField] private Animator animator;
    [SerializeField] private UnitStatsRuntime currentEnemy;
    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerManager playerManager;

    // Rewards System
    [SerializeField] private RewardController rewardController;
    [SerializeField] private float rewardDelay = 1.5f;

    // Enemy Spawning + Selection
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyBattleUIHandler enemyBattleUIHandler;
    [SerializeField] private RaycastSelectionManager selectionManager;
    [SerializeField] private BattleUnit postRewardSkeletonPrefab;
    [SerializeField] private Transform postRewardSkeletonSpawnPoint;

    private bool waitingForRewardChoice = false;
    private bool postRewardSkeletonSpawned = false;

    private bool cont = false;
    private bool battleEnded = false;

    public Move enemyMove;

    // Multi-enemy support
    List<BattleUnit> spawnedEnemyUnits = new List<BattleUnit>();
    List<UnitStatsRuntime> enemyStatsRuntimes = new List<UnitStatsRuntime>();

    private BattleUnit selectedEnemy;

    private void Start()
    {
        // Spawn 3 enemies
        for (int i = 0; i < 3; i++)
        {
            BattleUnit spawnedEnemyUnit = enemySpawner.Spawn();
            if (spawnedEnemyUnit == null)
            {
                Debug.LogError("BattleManager: EnemySpawner failed to spawn an enemy.");
                return;
            }

            spawnedEnemyUnits.Add(spawnedEnemyUnit);

            // Get runtime stats
            UnitStatsRuntime stats = spawnedEnemyUnit.GetComponent<UnitStatsRuntime>();
            if (stats == null)
            {
                Debug.LogError("BattleManager: Spawned enemy has no UnitStatsRuntime.");
                return;
            }

            enemyStatsRuntimes.Add(stats);

            // Subscribe to death event
            stats.OnDied += HandleEnemyDied;

            // Bind UI
            EnemyWorldUIHandler worldUI = spawnedEnemyUnit.GetComponentInChildren<EnemyWorldUIHandler>();
            if (worldUI != null)
                worldUI.Bind(stats);
        }

        StartBattle();
    }

    private void StartBattle()
    {
        // Reset reward UI
        if (rewardController != null)
        {
            rewardController.HideRewardUI();
        }

        battleEnded = false;

        turnController.OnBattleStart();
    }

    private void OnDestroy()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnDied -= HandleEnemyDied;
        }
    }

    public bool AttackSelected(Move move)
    {
        if (battleEnded) return false;

        if (move == null)
        {
            Debug.LogWarning("BattleManager: Selected move was null.");
            return false;
        }

        // Get selected enemy
        selectedEnemy = selectionManager.GetSelectedEnemy();

        if (selectedEnemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy selected.");
            return false; 
        }

        currentEnemy = selectedEnemy.GetEnemy();

        if (currentEnemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy to attack.");
            return false;
        }

        // Perform attack
        float remainingHealth = currentEnemy.ApplyDamage(move.getDamage());
        float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());

        //do the attack animation
        animator.SetTrigger("AttackTrigger");

        Debug.Log("SelectedEnemy: " + selectedEnemy);
        Debug.Log("currentEnemy: " + currentEnemy);

        return true; 
    }

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        if (battleEnded) return;

        RemoveFromBattle(deadEnemy);
        ParticlePlayer.Instance.PlayParticleEffect();

        if (enemyStatsRuntimes.Count > 0)
        {
            if (enemyStatsRuntimes.Count == 1)
            {
                BattleUnit lastEnemy = spawnedEnemyUnits[0];
                HandleOneEnemyLeft(lastEnemy);
            }

            return;
        }

        // First wave cleared -> show rewards
        if (!postRewardSkeletonSpawned)
        {
            waitingForRewardChoice = true;
            battleEnded = true;
            StartCoroutine(HandleEnemyDefeatRoutine());
            return;
        }

        // Skeleton defeated -> real end, no second reward
        battleEnded = true;
        turnController.StopBattleFlow();
        Debug.Log("Skeleton defeated. Battle complete.");
    }
public void BeginPostRewardSkeletonFight()
    {
        if (!waitingForRewardChoice || postRewardSkeletonSpawned)
            return;

        if (postRewardSkeletonPrefab == null || postRewardSkeletonSpawnPoint == null)
        {
            Debug.LogError("BattleManager: Missing skeleton prefab or spawn point.");
            return;
        }

        waitingForRewardChoice = false;
        postRewardSkeletonSpawned = true;

        if (rewardController != null)
            rewardController.HideRewardUI();

        BattleUnit spawnedEnemyUnit = Instantiate(
            postRewardSkeletonPrefab,
            postRewardSkeletonSpawnPoint.position,
            postRewardSkeletonSpawnPoint.rotation
        );

        spawnedEnemyUnits.Add(spawnedEnemyUnit);

        UnitStatsRuntime stats = spawnedEnemyUnit.GetComponent<UnitStatsRuntime>();
        if (stats == null)
        {
            Debug.LogError("BattleManager: Skeleton has no UnitStatsRuntime.");
            return;
        }

        enemyStatsRuntimes.Add(stats);
        stats.OnDied += HandleEnemyDied;

        EnemyWorldUIHandler worldUI = spawnedEnemyUnit.GetComponentInChildren<EnemyWorldUIHandler>();
        if (worldUI != null)
            worldUI.Bind(stats);

        EnemyManager.Instance.RegisterEnemy(spawnedEnemyUnit);

        currentEnemy = stats;
        selectedEnemy = spawnedEnemyUnit;
        selectionManager.KeepSelected(spawnedEnemyUnit);

        battleEnded = false;
        StartCoroutine(RestartBattleFlowNextFrame());
    }

    private IEnumerator RestartBattleFlowNextFrame()
    {
        yield return null;
        turnController.OnBattleStart();
    }

    private IEnumerator HandleEnemyDefeatRoutine()
    {
        Debug.Log("All enemies defeated.");

        turnController.StopBattleFlow();

        if (ParticlePlayer.Instance != null)
            ParticlePlayer.Instance.PlayParticleEffect();

        yield return new WaitForSeconds(rewardDelay);

        if (rewardController != null)
            rewardController.ShowRewardUI();
    }

    public Move getMove(int i)
    {
        return PlayerManager.Instance.GetMove(i);
    }

    public void EnemyAttack(BattleUnit enemy)
    {
        if (battleEnded) return;

        if (enemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy to perform attack.");
            return;
        }

        StartCoroutine(EnemyAttackRoutine(enemy));
    }

    private IEnumerator EnemyAttackRoutine(BattleUnit enemy)
    {
        cont = false;
        enemyMove = enemy.GetEnemy().GetEnemyData().GetRandomMove();

        // Show attack message on screen-space UI
        enemyBattleUIHandler.ShowEnemyAttackMessage(enemy.GetName(), enemyMove);

        EnemyAnimatorHandler enemyAnim = enemy.GetComponent<EnemyAnimatorHandler>();
        if (enemyAnim != null)
        {
            enemyAnim.PlayAttack();
        }

        yield return new WaitUntil(() => cont);

        if (battleEnded) yield break;

        float remainingHealth = playerManager.ApplyDamage(enemyMove.getDamage());

        if (!battleEnded && remainingHealth <= 0)
        {
            battleEnded = true;
            turnController.StopBattleFlow();
            StartCoroutine(HandlePlayerDefeat());
        }
    }

    private IEnumerator HandlePlayerDefeat()
{
    Debug.Log("Player defeated.");

    yield return new WaitForSeconds(1f);

    SceneTransitionManager.LoadSceneWithTransition(deathSceneName);
}


    public void ContinueBattle()
    {
        if (battleEnded) return;
        cont = true;
    }

private void RemoveFromBattle(UnitStatsRuntime unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("BattleManager: Attempted to remove null unit from battle.");
            return;
        }

        BattleUnit battleUnit = unit.GetComponent<BattleUnit>();

        int index = enemyStatsRuntimes.IndexOf(unit);
        if (index >= 0)
        {
            enemyStatsRuntimes.RemoveAt(index);
            spawnedEnemyUnits.RemoveAt(index);
        }

        EnemyManager.Instance.UnregisterEnemy(battleUnit);
        turnController.RemoveBattleUnit(battleUnit);
        Destroy(unit.gameObject);
    }

    public void HandleOneEnemyLeft(BattleUnit enemy)
    {
        currentEnemy = enemy.GetEnemy();
        selectedEnemy = enemy;
        selectionManager.KeepSelected(enemy);
    }
}
