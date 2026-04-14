using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private BattleUnit currentEnemyUnit;
    [SerializeField] private UnitStatsRuntime currentEnemy;
    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyBattleUIHandler enemyBattleUIHandler;
    private bool cont = false;

    public Move enemyMove;


    private void Start()
    {
        // 1. Spawn enemy
        BattleUnit spawnedEnemy = enemySpawner.Spawn();

        if (spawnedEnemy == null)
        {
            Debug.LogError("BattleManager: EnemySpawner failed to spawn an enemy.");
            return;
        }

        // 2. Cache references
        currentEnemyUnit = spawnedEnemy;
        currentEnemy = spawnedEnemy.GetEnemy();

        if (currentEnemy == null)
        {
            Debug.LogError("BattleManager: Spawned enemy has no UnitStatsRuntime.");
            return;
        }

        // 3. Hook death event
        currentEnemy.OnDied += HandleEnemyDied;

        // 4. Bind world-space UI (floating health bar)
        EnemyWorldUIHandler worldUI = spawnedEnemy.GetComponentInChildren<EnemyWorldUIHandler>();
        if (worldUI != null)
            worldUI.Bind(currentEnemy);

        // 5. Start the battle AFTER everything is ready
        StartBattle();
    }

    private void StartBattle()
    {
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

        if (currentEnemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy to attack.");
            return;
        }

        Debug.Log("Performing Attack: " + move.name);


        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }

        float remainingHealth = currentEnemy.ApplyDamage(move.getDamage());
        float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());
        Debug.Log("Player used " + move.getMoveName() + ". Remaining Mana: " + remainingMana);
        Debug.Log("Enemy took " + move.getDamage()+ " damage. Remaining HP: " + remainingHealth);


    }

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        Debug.Log("Enemy defeated.");

        if (deadEnemy != null)
        {
            Destroy(deadEnemy.gameObject);
            ParticlePlayer.Instance.PlayParticleEffect();
        }

        currentEnemy = null;
    }

    public Move getMove(int i)
    {
        return PlayerManager.Instance.GetMove(i);
    }

    public void EnemyAttack(BattleUnit enemy)
    {
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

        playerManager.ApplyDamage(enemyMove.getDamage());
    }

    public void ContinueBattle()
    {
        cont = true;
    }
}