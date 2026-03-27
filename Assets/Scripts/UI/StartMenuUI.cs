using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    [SerializeField] private string fightSceneName = "FightScene";

    public void StartGame()
    {
        SceneManager.LoadScene(fightSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed.");
        Application.Quit();
    }
}