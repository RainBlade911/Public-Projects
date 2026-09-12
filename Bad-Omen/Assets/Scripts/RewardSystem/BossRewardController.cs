using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRewardController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject bossRewardUI;

    [Header("Scene")]
    [SerializeField] private string fightSceneName = "FightScene";

    [Header("Fade")]
    [SerializeField] private CanvasGroup bossRewardCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    private void Awake()
    {
        HideBossRewardUI();
    }

    public void ShowBossRewardUI()
    {
        StartCoroutine(ShowBossRewardRoutine());
    }

    private IEnumerator ShowBossRewardRoutine()
    {
        if (bossRewardUI != null)
        {
            bossRewardUI.SetActive(true);
            bossRewardUI.transform.SetAsLastSibling();
        }

        if (bossRewardCanvasGroup != null)
        {
            bossRewardCanvasGroup.alpha = 0f;
            bossRewardCanvasGroup.interactable = false;
            bossRewardCanvasGroup.blocksRaycasts = false;

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                bossRewardCanvasGroup.alpha =
                    Mathf.Lerp(0f, 1f, timer / fadeDuration);

                yield return null;
            }

            bossRewardCanvasGroup.alpha = 1f;
            bossRewardCanvasGroup.interactable = true;
            bossRewardCanvasGroup.blocksRaycasts = true;
        }

        Debug.Log("Boss Reward UI shown.");
    }

    public void HideBossRewardUI()
    {
        if (bossRewardCanvasGroup != null)
        {
            bossRewardCanvasGroup.alpha = 0f;
            bossRewardCanvasGroup.interactable = false;
            bossRewardCanvasGroup.blocksRaycasts = false;
        }

        if (bossRewardUI != null)
            bossRewardUI.SetActive(false);
    }

    private IEnumerator HideBossRewardRoutine()
    {
        if (bossRewardCanvasGroup == null)
        {
            if (bossRewardUI != null)
                bossRewardUI.SetActive(false);

            yield break;
        }

        bossRewardCanvasGroup.interactable = false;
        bossRewardCanvasGroup.blocksRaycasts = false;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            bossRewardCanvasGroup.alpha =
                Mathf.Lerp(1f, 0f, timer / fadeDuration);

            yield return null;
        }

        bossRewardCanvasGroup.alpha = 0f;

        if (bossRewardUI != null)
            bossRewardUI.SetActive(false);
    }

    public void ApplyEssenceOfStrength()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("BossRewardController: No PlayerManager instance found.");
            return;
        }

        PlayerManager.Instance.UpgradeEOS();

        StartCoroutine(ApplyRewardAndReloadRoutine());
    }

    public void ApplyEssenceOfKnowledge()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("BossRewardController: No PlayerManager instance found.");
            return;
        }

        PlayerManager.Instance.UpgradeEOK();

        StartCoroutine(ApplyRewardAndReloadRoutine());
    }

    private IEnumerator ApplyRewardAndReloadRoutine()
    {
        yield return StartCoroutine(HideBossRewardRoutine());

        ReloadFightScene();
    }

    private void ReloadFightScene()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SavePersistentState();
        }

        Debug.Log("BossRewardController: Reloading fight scene...");
        SceneManager.LoadScene(fightSceneName);
    }
}