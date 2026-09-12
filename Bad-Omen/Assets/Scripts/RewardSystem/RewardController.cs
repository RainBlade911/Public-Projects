using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardController : MonoBehaviour
{
    [Header("Moveset Reward Pool")]
    [SerializeField] private List<Move> moveRewardPool = new();

    [Header("Support Item Reward Pool")]
    [SerializeField] private List<string> supportItemRewardPool = new() { "Health Potion", "Mana Potion", "Elixir Potion" };

    [Header("UI")]
    [SerializeField] private GameObject rewardUI;
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private string fightSceneName = "FightScene";

    [Header("Reward Wheel")]
    [SerializeField] private RewardWheelSpinner rewardWheelSpinner;

    [Header("Reward Transition")]
    [SerializeField] private float rewardDelayBetweenChoices = 0.5f;
    [SerializeField] private float fadeDuration = 0.25f;

    private int pendingRewardChoices = 1;
    private bool rewardChoiceLocked = false;

    private void Awake()
    {
        if (rewardUI != null && rewardCanvasGroup == null)
            rewardCanvasGroup = rewardUI.GetComponent<CanvasGroup>();

        if (rewardWheelSpinner != null)
            rewardWheelSpinner.HideWheel();
    }

    public void SetPendingRewardChoices(int amount)
    {
        pendingRewardChoices = Mathf.Max(1, amount);
        Debug.Log("RewardController: Pending normal reward choices set to " + pendingRewardChoices);
    }

    public void ShowRewardUI()
    {
        StartCoroutine(ShowRewardRoutine());
    }

    private IEnumerator ShowRewardRoutine()
    {
        rewardChoiceLocked = false;

        if (rewardUI != null)
        {
            rewardUI.SetActive(true);
            rewardUI.transform.SetAsLastSibling();
        }

        if (rewardCanvasGroup != null)
        {
            rewardCanvasGroup.alpha = 0f;
            rewardCanvasGroup.interactable = false;
            rewardCanvasGroup.blocksRaycasts = false;

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                rewardCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            rewardCanvasGroup.alpha = 1f;
            rewardCanvasGroup.interactable = true;
            rewardCanvasGroup.blocksRaycasts = true;
        }

        Debug.Log("Reward UI shown.");
    }

    public void HideRewardUI()
    {
        if (rewardUI != null)
            rewardUI.SetActive(false);

        if (rewardCanvasGroup != null)
        {
            rewardCanvasGroup.alpha = 0f;
            rewardCanvasGroup.interactable = false;
            rewardCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ApplyRandomEquipmentUpgrade()
    {
        if (rewardChoiceLocked) return;
        rewardChoiceLocked = true;

        StartCoroutine(ApplyRandomEquipmentUpgradeRoutine());
    }

    private IEnumerator ApplyRandomEquipmentUpgradeRoutine()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            yield break;
        }

        string chosenRewardName = GetRandomEquipmentRewardName();

        if (chosenRewardName == "Health Reward")
        {
            PlayerManager.Instance.IncreaseMaxHealth(5f);
            Debug.Log("Equipment Upgrade: Player gained +5 Max Health.");
        }
        else if (chosenRewardName == "Mana Reward")
        {
            PlayerManager.Instance.IncreaseMaxMana(5f);
            Debug.Log("Equipment Upgrade: Player gained +5 Max Mana.");
        }
        else if (chosenRewardName == "Combo Reward")
        {
            PlayerManager.Instance.IncreaseMaxHealth(5f);
            PlayerManager.Instance.IncreaseMaxMana(5f);
            Debug.Log("Equipment Upgrade: Player gained +5 Max Health and +5 Max Mana.");
        }

        yield return StartCoroutine(FadeOutRewardUI());
        yield return StartCoroutine(PlayRewardWheelIfPossible(RewardWheelType.Equipment, chosenRewardName));
        yield return StartCoroutine(FinishOneRewardChoiceRoutine());
    }

    public void ApplyRandomMovesetUpgrade()
    {
        if (rewardChoiceLocked) return;
        rewardChoiceLocked = true;

        StartCoroutine(ApplyRandomMovesetUpgradeRoutine());
    }

    private IEnumerator ApplyRandomMovesetUpgradeRoutine()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            yield break;
        }

        if (moveRewardPool == null || moveRewardPool.Count == 0)
        {
            Debug.LogWarning("RewardController: No moves assigned in Move Reward Pool.");
            yield return StartCoroutine(FadeOutRewardUI());
            yield return StartCoroutine(FinishOneRewardChoiceRoutine());
            yield break;
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
            yield return StartCoroutine(FadeOutRewardUI());
            yield return StartCoroutine(FinishOneRewardChoiceRoutine());
            yield break;
        }

        Move chosenMove = availableMoves[Random.Range(0, availableMoves.Count)];
        PlayerManager.Instance.AddMove(chosenMove);

        Debug.Log(
            "Moveset Upgrade: Player learned " +
            chosenMove.getMoveName() +
            " | Damage: " + chosenMove.getDamage() +
            " | Mana Cost: " + chosenMove.getManaCost()
        );

        yield return StartCoroutine(FadeOutRewardUI());
        yield return StartCoroutine(PlayRewardWheelIfPossible(RewardWheelType.Moveset, chosenMove.getMoveName()));
        yield return StartCoroutine(FinishOneRewardChoiceRoutine());
    }

    public void ApplyRandomSupportItemUpgrade()
    {
        if (rewardChoiceLocked) return;
        rewardChoiceLocked = true;

        StartCoroutine(ApplyRandomSupportItemUpgradeRoutine());
    }

    private IEnumerator ApplyRandomSupportItemUpgradeRoutine()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            yield break;
        }

        if (supportItemRewardPool == null || supportItemRewardPool.Count == 0)
        {
            Debug.LogWarning("RewardController: No support items assigned in Support Item Reward Pool.");
            yield return StartCoroutine(FadeOutRewardUI());
            yield return StartCoroutine(FinishOneRewardChoiceRoutine());
            yield break;
        }

        string chosenItem = supportItemRewardPool[Random.Range(0, supportItemRewardPool.Count)];
        PlayerManager.Instance.AddItem(chosenItem);

        Debug.Log("Support Item Upgrade: Player received " + chosenItem + ".");

        yield return StartCoroutine(FadeOutRewardUI());
        yield return StartCoroutine(PlayRewardWheelIfPossible(RewardWheelType.Support, chosenItem));
        yield return StartCoroutine(FinishOneRewardChoiceRoutine());
    }

    private string GetRandomEquipmentRewardName()
    {
        int roll = Random.Range(0, 7);

        switch (roll)
        {
            case 0:
                return "Mana Reward";

            case 1:
                return "Health Reward";

            case 2:
                return "Mana Reward";

            case 3:
                return "Health Reward";

            case 4:
                return "Mana Reward";

            case 5:
                return "Combo Reward";

            default:
                return "Health Reward";
        }
    }

    private IEnumerator PlayRewardWheelIfPossible(RewardWheelType wheelType, string rewardName)
    {
        if (rewardWheelSpinner == null)
            yield break;

        yield return StartCoroutine(rewardWheelSpinner.PlayWheel(wheelType, rewardName));
    }

    private IEnumerator FinishOneRewardChoiceRoutine()
    {
        pendingRewardChoices--;

        if (pendingRewardChoices > 0)
        {
            Debug.Log("RewardController: More rewards remaining: " + pendingRewardChoices);

            yield return new WaitForSeconds(rewardDelayBetweenChoices);

            yield return StartCoroutine(ShowRewardRoutine());
            yield break;
        }

        ReloadFightScene();
    }

    private IEnumerator FadeOutRewardUI()
    {
        if (rewardCanvasGroup == null)
        {
            if (rewardUI != null)
                rewardUI.SetActive(false);

            yield break;
        }

        rewardCanvasGroup.interactable = false;
        rewardCanvasGroup.blocksRaycasts = false;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            rewardCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        rewardCanvasGroup.alpha = 0f;

        if (rewardUI != null)
            rewardUI.SetActive(false);
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