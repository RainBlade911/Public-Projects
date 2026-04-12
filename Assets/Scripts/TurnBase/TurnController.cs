using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class TurnController : MonoBehaviour
{

    private List<BattleUnit> battleUnits = new();
    private List<BattleUnit> orderedUnits = new();
    private int turnIndex = 0;
    [SerializeField] private PlayerMenus playerMenus;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private EnemyUIHandler enemyUIHandler;
    [SerializeField] private ButtonScript button;
    [SerializeField] private PlayerActionUI playerActionUI;
    [SerializeField] private TurnOrderSlider turnOrderSlider;

    private bool actionSelected = false;


    public void OnBattleStart()
    {
        DetermineTurnOrder();
        turnOrderSlider.UpdateTurnOrder(turnOrderToUI());
        // After bubble sorting orderedUnits
        Debug.Log("=== TURN ORDER ===");
        for (int i = 0; i < orderedUnits.Count; i++)
        {
            Debug.Log($"{i + 1}: {orderedUnits[i].GetName()} (SPD: {orderedUnits[i].GetSpeed()})");
        }
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

    private void StartNextTurn()
    {
        BattleUnit currentUnit = orderedUnits[turnIndex];

        Debug.Log($"It's {currentUnit.GetName()}'s turn!");

        turnOrderSlider.UpdateTurnOrder(turnOrderToUI());

        if (currentUnit.IsPlayer())
        {
            StartPlayerTurn(currentUnit);
        }
        else
        {
            StartEnemyTurn(currentUnit);
        }

        turnIndex = (turnIndex + 1) % orderedUnits.Count;

    }

    private void EndTurn()
    {
        // If we just finished the last unit in the round
        if (turnIndex == orderedUnits.Count)
        {
            turnIndex = 0;
            DetermineTurnOrder();
        }

        StartNextTurn();
    }

    private void DetermineTurnOrder()
    {
        battleUnits.AddRange(FindObjectsByType<BattleUnit>(FindObjectsSortMode.None));

        // Sort battleUnits based on speed or other criteria to determine turn order
        orderedUnits.Clear();
        orderedUnits.AddRange(battleUnits);
        for (int i = 0; i < orderedUnits.Count - 1; i++)
        {
            for (int j = 0; j < orderedUnits.Count - i - 1; j++)
            {
                if (orderedUnits[j].GetSpeed() < orderedUnits[j + 1].GetSpeed())
                {
                    BattleUnit temp = orderedUnits[j];
                    orderedUnits[j] = orderedUnits[j + 1];
                    orderedUnits[j + 1] = temp;
                }
            }
        }
    }

    private void StartPlayerTurn(BattleUnit player)
    {
        Debug.Log("Player's turn started. Awaiting player action...");
        StartCoroutine(PlayerActionRoutine(player));

    }

    private IEnumerator PlayerActionRoutine(BattleUnit player)
    {
        actionSelected = false;
        playerMenus.ChangeUITo("Default");
        yield return new WaitUntil(() => actionSelected);
        playerMenus.SetAllInactive();
        playerActionUI.SetTextActive();
        yield return new WaitForSeconds(1f);
        Debug.Log("Player action completed.");

        EndTurn();

    }

    private void StartEnemyTurn(BattleUnit enemy)
    {
        Debug.Log("ENEMY TURN: " + enemy.GetName());

        StartCoroutine(EnemyActionRoutine(enemy));
    }

    private IEnumerator EnemyActionRoutine(BattleUnit enemy)
    {
        battleManager.EnemyAttack(enemy);
        yield return new WaitForSeconds(2f); // Simulate thinking time
        playerActionUI.HideText();
        playerMenus.SetAllInactive();
        enemyUIHandler.StartEnemyMessage(battleManager.enemyMove);
        yield return new WaitUntil(() => enemyUIHandler.Continue);
        Debug.Log($"{enemy.GetName()} attacks!");

        playerMenus.ChangeUITo("Default");

        EndTurn();
    }

    public void ActionSelected()
    {
        actionSelected = true;
    }

}
