using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRewardController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject bossRewardUI;

    [Header("Scene")]
    [SerializeField] private string fightSceneName = "FightScene";

    public void ShowBossRewardUI()
    {
        if (bossRewardUI != null)
        {
            bossRewardUI.SetActive(true);
            bossRewardUI.transform.SetAsLastSibling();
        }

        Debug.Log("Boss Reward UI shown.");
    }

    public void HideBossRewardUI()
    {
        if (bossRewardUI != null)
        {
            bossRewardUI.SetActive(false);
        }
    }

    public void ApplyEssenceOfStrength()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("BossRewardController: No PlayerManager instance found.");
            return;
        }

        PlayerManager.Instance.UpgradeEOS();
        ReloadFightScene();
    }

    public void ApplyEssenceOfKnowledge()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("BossRewardController: No PlayerManager instance found.");
            return;
        }

        PlayerManager.Instance.UpgradeEOK();
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