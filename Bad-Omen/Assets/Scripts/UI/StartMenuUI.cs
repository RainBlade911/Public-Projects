
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private string fightSceneName = "FightScene";

    public void StartGame()
    {
        Debug.Log("Start button clicked");
        SceneTransitionManager.LoadSceneWithTransition(fightSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed.");
        Application.Quit();
    }
}