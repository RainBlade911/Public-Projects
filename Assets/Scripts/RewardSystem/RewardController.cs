using System.Collections;
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
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private string fightSceneName = "FightScene";

    [Header("Reward Transition")]
    [SerializeField] private float rewardDelayBetweenChoices = 0.5f;
    [SerializeField] private float fadeDuration = 0.25f;

    private int pendingRewardChoices = 1;
    private bool rewardChoiceLocked = false;

    private void Awake()
    {
        if (rewardUI != null && rewardCanvasGroup == null)
            rewardCanvasGroup = rewardUI.GetComponent<CanvasGroup>();
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

        StartCoroutine(FinishOneRewardChoiceRoutine());
    }

    public void ApplyRandomMovesetUpgrade()
    {
        if (rewardChoiceLocked) return;
        rewardChoiceLocked = true;

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            return;
        }

        if (moveRewardPool == null || moveRewardPool.Count == 0)
        {
            Debug.LogWarning("RewardController: No moves assigned in Move Reward Pool.");
            StartCoroutine(FinishOneRewardChoiceRoutine());
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
            StartCoroutine(FinishOneRewardChoiceRoutine());
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

        StartCoroutine(FinishOneRewardChoiceRoutine());
    }

    public void ApplyRandomSupportItemUpgrade()
    {
        if (rewardChoiceLocked) return;
        rewardChoiceLocked = true;

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardController: No PlayerManager instance found.");
            return;
        }

        if (supportItemRewardPool == null || supportItemRewardPool.Count == 0)
        {
            Debug.LogWarning("RewardController: No support items assigned in Support Item Reward Pool.");
            StartCoroutine(FinishOneRewardChoiceRoutine());
            return;
        }

        string chosenItem = supportItemRewardPool[Random.Range(0, supportItemRewardPool.Count)];
        PlayerManager.Instance.AddItem(chosenItem);

        Debug.Log("Support Item Upgrade: Player received " + chosenItem + ".");

        StartCoroutine(FinishOneRewardChoiceRoutine());
    }

    private IEnumerator FinishOneRewardChoiceRoutine()
    {
        yield return StartCoroutine(FadeOutRewardUI());

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