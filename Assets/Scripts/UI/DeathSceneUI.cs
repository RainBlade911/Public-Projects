using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;

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

        SceneTransitionManager.LoadSceneWithTransition(fightSceneName);
    }

    public void GoToMainMenu()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.ClearState();
        }

        SceneTransitionManager.LoadSceneWithTransition(mainMenuSceneName);
    }
}