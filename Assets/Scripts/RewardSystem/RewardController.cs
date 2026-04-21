using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardController : MonoBehaviour
{
    [Header("Moveset Reward Pool")]
    [SerializeField] private List<Move> moveRewardPool = new();

    [Header("Support Item Reward Pool")]
    [SerializeField] private List<string> supportItemRewardPool = new() { "Health Potion", "Mana Potion" };

    [Header("UI")]
    [SerializeField] private GameObject rewardUI;
    [SerializeField] private string fightSceneName = "FightScene";
    [SerializeField] private BattleManager battleManager;

    public void ShowRewardUI()
    {
        if (rewardUI != null)
        {
            rewardUI.SetActive(true);
            rewardUI.transform.SetAsLastSibling();
        }

        Debug.Log("Reward UI shown.");
    }

    public void HideRewardUI()
    {
        if (rewardUI != null)
        {
            rewardUI.SetActive(false);
        }
    }

    public void ApplyRandomEquipmentUpgrade()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            return;
        }

        int roll = Random.Range(0, 2);

        if (roll == 0)
        {
            PlayerManager.Instance.IncreaseMaxHealth(5f);
            Debug.Log("Equipment Upgrade: Player gained +5 Max Health.");
        }
        else
        {
            PlayerManager.Instance.IncreaseMaxMana(5f);
            Debug.Log("Equipment Upgrade: Player gained +5 Max Mana.");
        }

        CompleteRewardStep();
    }

    private void CompleteRewardStep()
    {
        if (rewardUI != null)
            rewardUI.SetActive(false);

        if (battleManager != null)
            battleManager.BeginPostRewardSkeletonFight();
        else
            Debug.LogError("RewardController: BattleManager is not assigned.");
    }

    public void ApplyRandomMovesetUpgrade()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            return;
        }

        if (moveRewardPool == null || moveRewardPool.Count == 0)
        {
            Debug.LogWarning("RewardController: No moves assigned in Move Reward Pool.");
            CompleteRewardStep();
            return;
        }

        List<Move> availableMoves = new List<Move>();

        foreach (Move move in moveRewardPool)
        {
            if (move != null && !PlayerManager.Instance.moves.Contains(move))
            {
                availableMoves.Add(move);
            }
        }

        if (availableMoves.Count == 0)
        {
            Debug.Log("Moveset Upgrade: No new moves available. Player already has all reward moves.");
            CompleteRewardStep();
            return;
        }

        Move chosenMove = availableMoves[Random.Range(0, availableMoves.Count)];
        PlayerManager.Instance.AddMove(chosenMove);

        Debug.Log(
            "Moveset Upgrade: Player learned " +
            chosenMove.getMoveName() +
            " | Damage: " + chosenMove.getDamage() +
            " | Mana Cost: " + chosenMove.getManaCost()
        );

        CompleteRewardStep();
    }

    public void ApplyRandomSupportItemUpgrade()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            return;
        }

        if (supportItemRewardPool == null || supportItemRewardPool.Count == 0)
        {
            Debug.LogWarning("RewardController: No support items assigned in Support Item Reward Pool.");
            CompleteRewardStep();
            return;
        }

        string chosenItem = supportItemRewardPool[Random.Range(0, supportItemRewardPool.Count)];
        PlayerManager.Instance.AddItem(chosenItem);

        Debug.Log("Support Item Upgrade: Player received " + chosenItem + ".");
        CompleteRewardStep();
    }

    public void ReloadFightScene()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SavePersistentState();
        }

        Debug.Log("Reloading fight scene...");
        SceneManager.LoadScene(fightSceneName);
    }
}