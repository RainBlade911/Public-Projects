using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private UnitStatsRuntime currentEnemy;
    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private RewardController rewardController;
    [SerializeField] private float rewardDelay = 1.5f;

    private bool cont = false;
    private bool battleEnded = false;

    public Move enemyMove;

    private void Start()
    {
        StartBattle();
    }

    private void StartBattle()
    {
        if (currentEnemy == null)
        {
            Debug.LogError("BattleManager: No current enemy assigned.", this);
            return;
        }

        if (rewardController != null)
        {
            rewardController.HideRewardUI();
            // Removed due to moving to cleaner system: rewardController.ClearCurrentChoices();
        }

        battleEnded = false;
        turnController.OnBattleStart();
        currentEnemy.OnDied += HandleEnemyDied;
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
            Destroy(deadEnemy.gameObject);
        }

        currentEnemy = null;

        if (rewardController != null)
        {
            // UNREMOVE AFTER IMPLEMENTATION: rewardController.GenerateRewards();
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
}