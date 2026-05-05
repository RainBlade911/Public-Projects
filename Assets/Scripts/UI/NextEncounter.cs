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
    List<List<BattleUnit>> preparedEncounters = new List<List<BattleUnit>>();
    private EnemyEncounters enemyEncounter;
    private int enemyCount = 3;
    private string NextEncounterMessage;
    private bool clicked = false;

    public int SelectedEncounterIndex { get; private set; } = -1;




    public void ShowEncounterScreen()
    {
        preparedEncounters.Clear();

        for (int i = 0; i < EnemyContainers.Count; i++)
        {
            // Prepare encounter i
            List<BattleUnit> encounter = enemySpawner.PrepareSpawn();
            preparedEncounters.Add(encounter);

            // Get the UI script for this encounter card
            EnemyEncounters encounterUI = EnemyContainers[i].GetComponent<EnemyEncounters>();

            if (encounterUI == null)
            {
                Debug.LogError($"EnemyEncounters script missing on container {i}: {EnemyContainers[i].name}");
                continue;
            }

            // Fill UI for each enemy in this encounter
            bool isBoss = enemySpawner.IsPreparedEncounterBoss(i);

            for (int j = 0; j < encounter.Count; j++)
            {
                var data = encounter[j].GetEnemy().GetEnemyData();

                encounterUI.SetEnemyInfo(
                    j,
                    data.GetEnemyName(),
                    data.GetAffinity().GetAffinityName(),
                    data.GetSprite(),
                    isBoss
                );
            }

            // Hide unused slots
            encounterUI.HideUnusedSlots(encounter.Count);

            // Activate the container for this encounter
            EnemyContainers[i].SetActive(true);
        }


    }




    public void SelectEncounter(int index)
    {
        SelectedEncounterIndex = index;
        NextEncounterUI.SetActive(false);
        battleManager.BeginBattleAfterEncounter();
    }





}
