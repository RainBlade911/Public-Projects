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

    // ============================
    // BATTLE START
    // ============================

    public void OnBattleStart()
    {
        turnIndex = 0;
        Scorekeeper.Instance.IncrementRoundCounter();
        DetermineTurnOrder();
        turnOrderSlider.UpdateTurnOrder(TurnOrderToUI());

        Debug.Log("=== TURN ORDER ===");
        for (int i = 0; i < orderedUnits.Count; i++)
            Debug.Log($"{i + 1}: {orderedUnits[i].GetName()} (SPD: {orderedUnits[i].GetSpeed()})");
        Debug.Log("==================");

        StartNextTurn();
    }

    public void OnBattleEnded()
    {
        playerMenus.SetAllInactive();
        playerActionUI.HideText();
        Debug.Log("TurnController: Battle ended. Turn flow stopped.");
    }

    private Sprite[] TurnOrderToUI()
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
        if (battleManager.BattleEnded) return;
        if (orderedUnits.Count == 0) return;

        BattleUnit currentUnit = orderedUnits[turnIndex];

        Debug.Log($"It's {currentUnit.GetName()}'s turn!");

        turnOrderSlider.UpdateTurnOrder(TurnOrderToUI());

        if (currentUnit.IsPlayer())
            StartPlayerTurn(currentUnit);
        else
            StartEnemyTurn(currentUnit);

        turnIndex = (turnIndex + 1) % orderedUnits.Count;
    }

    private void EndTurn()
    {
        if (battleManager.BattleEnded) return;

        StartCoroutine(WaitAndStartNextTurn());
    }

    private IEnumerator WaitAndStartNextTurn()
    {
        // Wait for any attack and enemy UI to finish, or battle to end
        yield return new WaitUntil(() =>
            battleManager.BattleEnded ||
            (!battleManager.PlayerAttackInProgress &&
             !battleManager.EnemyAttackInProgress &&
             !battleManager.EnemyUIActive));

        if (battleManager.BattleEnded) yield break;

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
        if (battleManager.BattleEnded) return;

        Debug.Log("Player's turn started. Awaiting player action...");
        StartCoroutine(PlayerActionRoutine(player));
    }

    private IEnumerator PlayerActionRoutine(BattleUnit player)
    {
        actionSelected = false;
        playerMenus.ChangeUITo("Default");

        // Wait until the player chooses an action or the battle ends
        yield return new WaitUntil(() => actionSelected || battleManager.BattleEnded);

        if (battleManager.BattleEnded) yield break;

        playerMenus.SetAllInactive();

        Debug.Log("Player action selected. Waiting for attack to resolve...");

        EndTurn();
    }

    public void ActionSelected()
    {
        if (battleManager.BattleEnded) return;
        actionSelected = true;
    }

    // ============================
    // ENEMY TURN
    // ============================
    private void StartEnemyTurn(BattleUnit enemy)
    {
        if (battleManager.BattleEnded) return;

        Debug.Log("ENEMY TURN: " + enemy.GetName());
        StartCoroutine(EnemyActionRoutine(enemy));
    }

    private IEnumerator EnemyActionRoutine(BattleUnit enemy)
    {
        // Hide player menus as soon as enemy turn starts
        playerMenus.SetAllInactive();
        playerActionUI.HideText();

        enemyBattleUIHandler.ResetContinue();


        battleManager.EnemyAttack(enemy);

        // Wait for enemy UI "Continue" or battle end
        yield return new WaitUntil(() =>
            enemyBattleUIHandler.Continue || battleManager.BattleEnded);

        if (battleManager.BattleEnded) yield break;

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

            if (turnIndex >= orderedUnits.Count && orderedUnits.Count > 0)
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
