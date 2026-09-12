using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class DeathSceneUI : MonoBehaviour
{
    [SerializeField] private string fightSceneName = "FightScene";
    [SerializeField] private string mainMenuSceneName = "StartScene";
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private void Start()
    {
        finalScoreText.text = Scorekeeper.Instance.GetFinalScore().ToString("F0");
    }

    public void RetryGame()
    {
        Scorekeeper.Instance.ResetScore();
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.ClearState();
        }

        SceneTransitionManager.LoadSceneWithTransition(fightSceneName);
    }

    public void GoToMainMenu()
    {
        Scorekeeper.Instance.ResetScore();
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.ClearState();
        }

        SceneTransitionManager.LoadSceneWithTransition(mainMenuSceneName);
    }
}