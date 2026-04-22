using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    private List<BattleUnit> battleUnits = new();
    private List<BattleUnit> orderedUnits = new();
    private int turnIndex = 0;

    [SerializeField] private PlayerMenus playerMenus;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private PlayerActionUI playerActionUI;
    [SerializeField] private TurnOrderSlider turnOrderSlider;

    [SerializeField] private EnemyBattleUIHandler enemyBattleUIHandler;

    private bool actionSelected = false;
    private bool battleStopped = false;



    // ============================
    // BATTLE START
    // ============================

    public void OnBattleStart()
    {
        battleStopped = false;
        turnIndex = 0;

        DetermineTurnOrder();
        turnOrderSlider.UpdateTurnOrder(turnOrderToUI());

        Debug.Log("=== TURN ORDER ===");
        for (int i = 0; i < orderedUnits.Count; i++)
            Debug.Log($"{i + 1}: {orderedUnits[i].GetName()} (SPD: {orderedUnits[i].GetSpeed()})");
        Debug.Log("==================");

        StartNextTurn();
    }

    public void StopBattleFlow()
    {
        battleStopped = true;
        actionSelected = false;
        playerMenus.SetAllInactive();
        playerActionUI.HideText();
        Debug.Log("TurnController: Battle flow stopped.");
    }

    private Sprite[] turnOrderToUI()
    {
        Sprite[] sprites = new Sprite[orderedUnits.Count];

        for (int i = 0; i < orderedUnits.Count; i++)
        {
            int index = (turnIndex + i) % orderedUnits.Count;
            sprites[i] = orderedUnits[index].GetSprite();
        }

        return sprites;
    }

    // ============================
    // TURN FLOW
    // ============================
    private void StartNextTurn()
    {
        if (battleStopped) return;
        if (orderedUnits.Count == 0) return;

        BattleUnit currentUnit = orderedUnits[turnIndex];

        Debug.Log($"It's {currentUnit.GetName()}'s turn!");

        turnOrderSlider.UpdateTurnOrder(turnOrderToUI());

        if (currentUnit.IsPlayer())
            StartPlayerTurn(currentUnit);
        else
            StartEnemyTurn(currentUnit);

        turnIndex = (turnIndex + 1) % orderedUnits.Count;
    }

    private void EndTurn()
    {
        if (battleStopped) return;

        StartNextTurn();
    }

    // ============================
    // TURN ORDER SETUP
    // ============================
    private void DetermineTurnOrder()
    {
        battleUnits.Clear();

        // Add player
        battleUnits.Add(PlayerManager.Instance.GetBattleUnit());

        // Add all spawned enemies
        battleUnits.AddRange(EnemyManager.Instance.GetEnemies());

        // Sort by speed (descending)
        orderedUnits.Clear();
        orderedUnits.AddRange(battleUnits);
        orderedUnits.Sort((a, b) => b.GetSpeed().CompareTo(a.GetSpeed()));
    }

    // ============================
    // PLAYER TURN
    // ============================
    private void StartPlayerTurn(BattleUnit player)
    {
        if (battleStopped) return;

        Debug.Log("Player's turn started. Awaiting player action...");
        StartCoroutine(PlayerActionRoutine(player));
    }

    private IEnumerator PlayerActionRoutine(BattleUnit player)
    {
        actionSelected = false;
        playerMenus.ChangeUITo("Default");

        yield return new WaitUntil(() => actionSelected || battleStopped);

        if (battleStopped) yield break;

        playerMenus.SetAllInactive();
        playerActionUI.SetTextActive();

        yield return new WaitForSeconds(1f);

        if (battleStopped) yield break;

        playerActionUI.HideText();

        Debug.Log("Player action completed.");
        EndTurn();
    }

    public void ActionSelected()
    {
        if (battleStopped) return;
        actionSelected = true;
    }

    // ============================
    // ENEMY TURN
    // ============================
    private void StartEnemyTurn(BattleUnit enemy)
    {
        if (battleStopped) return;

        Debug.Log("ENEMY TURN: " + enemy.GetName());
        StartCoroutine(EnemyActionRoutine(enemy));
    }

    private IEnumerator EnemyActionRoutine(BattleUnit enemy)
    {
        battleManager.EnemyAttack(enemy);

        enemyBattleUIHandler.ShowEnemyAttackMessage(
            enemy.GetName(),
            battleManager.enemyMove
        );

        yield return new WaitUntil(() => enemyBattleUIHandler.Continue || battleStopped);

        if (battleStopped) yield break;

        playerActionUI.HideText();
        playerMenus.SetAllInactive();

        Debug.Log($"{enemy.GetName()} attacks!");


        EndTurn();
    }


    // ============================
    // UNIT REMOVAL
    // ============================
    public void RemoveBattleUnit(BattleUnit unit)
    {
        int removedIndex = orderedUnits.IndexOf(unit);

        battleUnits.Remove(unit);

        if (removedIndex >= 0)
        {
            orderedUnits.RemoveAt(removedIndex);

            if (removedIndex < turnIndex)
                turnIndex--;

            if (turnIndex >= orderedUnits.Count)
                turnIndex = 0;
        }

        if (orderedUnits.Count == 2)
            OneEnemyRemaining();
    }

    private void OneEnemyRemaining()
    {
        BattleUnit remainingEnemy = null;

        foreach (var unit in orderedUnits)
        {
            if (!unit.IsPlayer())
            {
                if (remainingEnemy != null)
                {
                    Debug.LogError("More than one enemy remaining! This should not happen.");
                    return;
                }
                remainingEnemy = unit;
            }
        }

        Debug.Log("Only one enemy remaining!");
        battleManager.HandleOneEnemyLeft(remainingEnemy);
    }
}