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

    // NEW: reference to the screen-space UI handler
    [SerializeField] private EnemyBattleUIHandler enemyBattleUIHandler;

    private bool actionSelected = false;

    // ============================
    // BATTLE START
    // ============================
    public void OnBattleStart()
    {
        DetermineTurnOrder();
        turnOrderSlider.UpdateTurnOrder(turnOrderToUI());

        Debug.Log("=== TURN ORDER ===");
        for (int i = 0; i < orderedUnits.Count; i++)
            Debug.Log($"{i + 1}: {orderedUnits[i].GetName()} (SPD: {orderedUnits[i].GetSpeed()})");
        Debug.Log("==================");

        StartNextTurn();
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
        Debug.Log("Player's turn started. Awaiting player action...");
        StartCoroutine(PlayerActionRoutine(player));
    }

    private IEnumerator PlayerActionRoutine(BattleUnit player)
    {
        actionSelected = false;
        playerMenus.ChangeUITo("Default");

        // Wait for player to pick a move
        yield return new WaitUntil(() => actionSelected);

        // Hide menus and show the attack message
        playerMenus.SetAllInactive();
        playerActionUI.SetTextActive();

        // Wait so the player can read the message
        yield return new WaitForSeconds(1f);

        // HIDE THE MESSAGE (this is the missing line)
        playerActionUI.HideText();

        Debug.Log("Player action completed.");

        EndTurn();
    }

    public void ActionSelected()
    {
        actionSelected = true;
    }

    // ============================
    // ENEMY TURN
    // ============================
    private void StartEnemyTurn(BattleUnit enemy)
    {
        Debug.Log("ENEMY TURN: " + enemy.GetName());
        StartCoroutine(EnemyActionRoutine(enemy));
    }

    private IEnumerator EnemyActionRoutine(BattleUnit enemy)
    {
        // Tell BattleManager to choose a move
        battleManager.EnemyAttack(enemy);

        // Show attack message on screen-space UI
        enemyBattleUIHandler.ShowEnemyAttackMessage(
            enemy.GetName(),
            battleManager.enemyMove
        );

        // Wait for Continue button
        yield return new WaitUntil(() => enemyBattleUIHandler.Continue);

        // Hide UI text
        playerActionUI.HideText();
        playerMenus.SetAllInactive();

        Debug.Log($"{enemy.GetName()} attacks!");

        playerMenus.ChangeUITo("Default");

        EndTurn();
    }

    public void RemoveBattleUnit(BattleUnit unit)
    {
        battleUnits.Remove(unit);
        orderedUnits.Remove(unit);
        // If the removed unit is before the current turn index, adjust the index
        int removedIndex = orderedUnits.IndexOf(unit);
        if (removedIndex >= 0 && removedIndex < turnIndex)
            turnIndex--;

        if (orderedUnits.Count == 2)
            OneEnemyRemaining();

    }

    private void OneEnemyRemaining()
    {
        BattleUnit remainingEnemy = null;
        for (int i = 0; i < orderedUnits.Count; i++)
        {
            if (!orderedUnits[i].IsPlayer())
            {

                if(remainingEnemy != null)
                {
                    Debug.LogError("More than one enemy remaining! This should not happen.");
                    return;
                }
                remainingEnemy = orderedUnits[i];
            }
        }
        Debug.Log("Only one enemy remaining!");

        battleManager.HandleOneEnemyLeft(remainingEnemy);
    }
}
