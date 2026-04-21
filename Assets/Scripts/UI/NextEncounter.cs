using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NextEncounter : MonoBehaviour
{
    [SerializeField] private GameObject NextEncounterUI;
    [SerializeField] private TextMeshProUGUI NextEncounterText;
    [SerializeField] private List<GameObject> EnemyContainers;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private EnemySpawner enemySpawner;
    private int enemyCount = 3;
    private string NextEncounterMessage;
    private bool clicked = false;



    public void ShowEncounterScreen()
    {
        List<BattleUnit> prepared = enemySpawner.PrepareSpawn();

        NextEncounterMessage = $"Danger! You encounter {prepared.Count} {(prepared.Count == 1 ? "enemy!" : "enemies!")}";
        NextEncounterText.text = NextEncounterMessage;

        for (int i = 0; i < EnemyContainers.Count; i++)
        {
            if (i < prepared.Count)
            {
                EnemyContainers[i].SetActive(true);
                EnemyContainers[i].GetComponentInChildren<TextMeshProUGUI>().text =
                    prepared[i].GetEnemy().GetEnemyData().GetEnemyName();
            }
            else
            {
                EnemyContainers[i].SetActive(false);
            }
        }

        StartCoroutine(OnClick());
    }


    public void OnEncounterContinueButton()
    {
        clicked = true;
    }

    public IEnumerator OnClick()
    {
        clicked = false;
        
        yield return new WaitUntil(() => clicked);

        NextEncounterUI.SetActive(false);

    }




}
