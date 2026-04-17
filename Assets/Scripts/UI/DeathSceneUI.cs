using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSceneUI : MonoBehaviour
{
    [SerializeField] private string fightSceneName = "FightScene";
    [SerializeField] private string mainMenuSceneName = "StartScene";

    public void RetryGame()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.ClearState();
        }

        SceneManager.LoadScene(fightSceneName);
    }

    public void GoToMainMenu()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.ClearState();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}