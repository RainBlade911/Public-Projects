using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{

    [SerializeField] Animator animator;
    private enum BattleState
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Busy,
        Won,
        Lost
    }

    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private List<BattleUnit> enemyUnits = new List<BattleUnit>();

    private BattleState currentState;

    private void Start()
    {
        StartBattle();
    }

    private void StartBattle()
    {
        if (playerUnit == null)
        {
            Debug.LogError("BattleManager: Player unit is missing.");
            return;
        }

        if (enemyUnits == null || enemyUnits.Count == 0)
        {
            Debug.LogError("BattleManager: No enemies assigned.");
            return;
        }

        currentState = BattleState.PlayerTurn;
        Debug.Log("Battle started. Player turn.");
    }

    public void OnPlayerAttackTarget(BattleUnit targetEnemy)
    {
        if (currentState != BattleState.PlayerTurn) return;
        if (targetEnemy == null) return;
        if (!targetEnemy.Health.IsAlive) return;

        StartCoroutine(PlayerAttackRoutine(targetEnemy));
    }

    private IEnumerator PlayerAttackRoutine(BattleUnit targetEnemy)
    {
        currentState = BattleState.Busy;

        ApplyStatusConditions(playerUnit);

        if (!playerUnit.Health.IsAlive)
        {
            currentState = BattleState.Lost;
            Debug.Log("Player defeated.");
            yield break;
        }

        if (targetEnemy == null || !targetEnemy.Health.IsAlive)
        {
            currentState = BattleState.PlayerTurn;
            yield break;
        }

        int damage = playerUnit.Stats.AttackPower;
        targetEnemy.Health.TakeDamage(damage);

        Debug.Log($"Player attacks {targetEnemy.UnitName} for {damage} damage.");

        yield return new WaitForSeconds(0.75f);

        if (AreAllEnemiesDefeated())
        {
            currentState = BattleState.Won;
            Debug.Log("All enemies defeated.");
            yield break;
        }

        currentState = BattleState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        currentState = BattleState.Busy;

        List<BattleUnit> livingEnemies = GetLivingEnemies();

        foreach (BattleUnit enemy in livingEnemies)
        {
            if (enemy == null || !enemy.Health.IsAlive) continue;

            ApplyStatusConditions(enemy);

            if (!enemy.Health.IsAlive)
            {
                continue;
            }

            if (!playerUnit.Health.IsAlive)
            {
                currentState = BattleState.Lost;
                Debug.Log("Player defeated.");
                yield break;
            }

            EnemyTurnAI ai = enemy.GetComponent<EnemyTurnAI>();
            int damage = 0;

            if (ai != null)
            {
                damage = ai.GetAttackDamage();
            }
            else if (enemy.Stats != null)
            {
                damage = enemy.Stats.AttackPower;
            }

            yield return new WaitForSeconds(0.75f);

            playerUnit.Health.TakeDamage(damage);
            Debug.Log($"{enemy.UnitName} attacks player for {damage} damage.");

            yield return new WaitForSeconds(0.75f);

            if (!playerUnit.Health.IsAlive)
            {
                currentState = BattleState.Lost;
                Debug.Log("Player defeated.");
                yield break;
            }
        }

        currentState = BattleState.PlayerTurn;
        Debug.Log("Player turn.");
    }

    private void ApplyStatusConditions(BattleUnit unit)
    {
        // Placeholder for future status effects.
    }

    private List<BattleUnit> GetLivingEnemies()
    {
        List<BattleUnit> livingEnemies = new List<BattleUnit>();

        foreach (BattleUnit enemy in enemyUnits)
        {
            if (enemy != null && enemy.Health != null && enemy.Health.IsAlive)
            {
                livingEnemies.Add(enemy);
            }
        }

        return livingEnemies;
    }

    private bool AreAllEnemiesDefeated()
    {
        foreach (BattleUnit enemy in enemyUnits)
        {
            if (enemy != null && enemy.Health != null && enemy.Health.IsAlive)
            {
                return false;
            }
        }

        return true;
    }

    public void AttackSelected()
    {
        Debug.Log("Performing Attack");
        animator.SetTrigger("AttackTrigger");

        //call attack stuff here
    }
}