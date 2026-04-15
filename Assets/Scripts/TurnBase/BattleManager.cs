using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
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

    public void AttackSelected(Move move)
    {
        if (battleEnded) return;

        if (move == null)
        {
            Debug.LogWarning("BattleManager: Selected move was null.");
            return;
        }

        // Get selected enemy
        selectedEnemy = selectionManager.GetSelectedEnemy();

        if (selectedEnemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy selected.");
            return;
        }

        currentEnemy = selectedEnemy.GetEnemy();

        if (currentEnemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy to attack.");
            return;
        }

        Debug.Log("Performing Attack: " + move.name);

        if (animator != null)
            animator.SetTrigger("AttackTrigger");

        float remainingHealth = currentEnemy.ApplyDamage(move.getDamage());
        float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());

        Debug.Log("Player used " + move.getMoveName() + ". Remaining Mana: " + remainingMana);
        Debug.Log("Enemy took " + move.getDamage() + " damage. Remaining HP: " + remainingHealth);
    }

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        if (battleEnded) return;

        battleEnded = true;
        StartCoroutine(HandleEnemyDefeatRoutine(deadEnemy));
    }

    private IEnumerator HandleEnemyDefeatRoutine(UnitStatsRuntime deadEnemy)
    {
        Debug.Log("Enemy defeated.");

        turnController.StopBattleFlow();

        if (ParticlePlayer.Instance != null)
        {
            ParticlePlayer.Instance.PlayParticleEffect();
        }

        yield return new WaitForSeconds(rewardDelay);

        if (deadEnemy != null)
        {
            Debug.Log("Removing enemy from battle and destroying game object.");
            RemoveFromBattle(deadEnemy);
            Destroy(deadEnemy.gameObject);
        }

        currentEnemy = null;

        if (rewardController != null)
        {
            rewardController.ShowRewardUI();
        }
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

        yield return new WaitUntil(() => cont);

        if (battleEnded) yield break;

        float remainingHealth = playerManager.ApplyDamage(enemyMove.getDamage());

        if (!battleEnded && remainingHealth <= 0)
        {
            battleEnded = true;
            turnController.StopBattleFlow();
            Debug.Log("Player defeated.");
        }
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

        turnController.RemoveBattleUnit(unit.GetComponent<BattleUnit>());
    }

    public void HandleOneEnemyLeft(BattleUnit enemy)
    {
        currentEnemy = enemy.GetEnemy();
        selectedEnemy = enemy;
        selectionManager.KeepSelected(enemy);
    }
}
