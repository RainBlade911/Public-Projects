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
    [SerializeField] private EnemyUIHandler enemyUIHandler;
    [SerializeField] private ButtonScript button;
    [SerializeField] private PlayerActionUI playerActionUI;
    [SerializeField] private TurnOrderSlider turnOrderSlider;

    private bool actionSelected = false;
    private bool battleStopped = false;

    public void OnBattleStart()
    {
        battleStopped = false;
        turnIndex = 0;

        DetermineTurnOrder();
        turnOrderSlider.UpdateTurnOrder(turnOrderToUI());

        Debug.Log("=== TURN ORDER ===");
        for (int i = 0; i < orderedUnits.Count; i++)
        {
            Debug.Log($"{i + 1}: {orderedUnits[i].GetName()} (SPD: {orderedUnits[i].GetSpeed()})");
        }
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

    private void StartNextTurn()
    {
        if (battleStopped) return;
        if (orderedUnits.Count == 0) return;

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
        if (battleStopped) return;

        if (turnIndex == orderedUnits.Count)
        {
            turnIndex = 0;
            DetermineTurnOrder();
        }

        StartNextTurn();
    }

    private void DetermineTurnOrder()
    {
        battleUnits.Clear();
        battleUnits.AddRange(FindObjectsByType<BattleUnit>(FindObjectsSortMode.None));

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

        Debug.Log("Player action completed.");
        EndTurn();
    }

    private void StartEnemyTurn(BattleUnit enemy)
    {
        if (battleStopped) return;

        Debug.Log("ENEMY TURN: " + enemy.GetName());
        StartCoroutine(EnemyActionRoutine(enemy));
    }

    private IEnumerator EnemyActionRoutine(BattleUnit enemy)
    {
        battleManager.EnemyAttack(enemy);

        yield return new WaitForSeconds(2f);

        if (battleStopped) yield break;

        playerActionUI.HideText();
        playerMenus.SetAllInactive();
        enemyUIHandler.StartEnemyMessage(battleManager.enemyMove);

        yield return new WaitUntil(() => enemyUIHandler.Continue || battleStopped);

        if (battleStopped) yield break;

        Debug.Log($"{enemy.GetName()} attacks!");

        playerMenus.ChangeUITo("Default");
        EndTurn();
    }

    public void ActionSelected()
    {
        if (battleStopped) return;
        actionSelected = true;
    }
}