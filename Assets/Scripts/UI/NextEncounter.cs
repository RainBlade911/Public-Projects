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
    //List<List<BattleUnit>> preparedEncounters = new List<List<BattleUnit>>();
    private EnemyEncounters enemyEncounter;
    private int enemyCount = 3;
    private string NextEncounterMessage;
    private bool clicked = false;

    public int SelectedEncounterIndex { get; private set; } = -1;




    public void ShowEncounterScreen()
    {
        // Do NOT clear a local list anymore
        // preparedEncounters.Clear();  // remove this line

        int roundCount = Scorekeeper.Instance.GetRoundCount() + 2;
        Debug.Log("Round Count is: " + roundCount);

        if (roundCount > 0 && roundCount % 5 == 0)
        {
            // Prepare boss encounter properly
            List<BattleUnit> bossEncounter = enemySpawner.PrepareBossEncounter();

            // Fill UI for container 0
            EnemyEncounters bossUI = EnemyContainers[0].GetComponent<EnemyEncounters>();
            var data = bossEncounter[0].GetEnemy().GetEnemyData();

            bossUI.SetEnemyInfo(
                0,
                data.GetEnemyName(),
                data.GetAffinity().GetAffinityName(),
                data.GetSprite(),
                true
            );

            bossUI.HideUnusedSlots(1);
            EnemyContainers[0].SetActive(true);

            // Hide the other encounter cards
            for (int i = 1; i < EnemyContainers.Count; i++)
                EnemyContainers[i].SetActive(false);

            return; // IMPORTANT: stop here so normal encounters don't generate
        }
        else
        {

            for (int i = 0; i < EnemyContainers.Count; i++)
            {
                List<BattleUnit> encounter = enemySpawner.PrepareSpawn(i);

                EnemyEncounters encounterUI = EnemyContainers[i].GetComponent<EnemyEncounters>();
                if (encounterUI == null)
                {
                    Debug.LogError($"EnemyEncounters script missing on container {i}: {EnemyContainers[i].name}");
                    continue;
                }

                // Fill UI for each enemy in this encounter
                //bool isBoss = enemySpawner.IsPreparedEncounterBoss(i);

                for (int j = 0; j < encounter.Count; j++)
                {
                    var data = encounter[j].GetEnemy().GetEnemyData();

                    encounterUI.SetEnemyInfo(
                        j,
                        data.GetEnemyName(),
                        data.GetAffinity().GetAffinityName(),
                        data.GetSprite(),
                        false
                    );
                }

                encounterUI.HideUnusedSlots(encounter.Count);
                EnemyContainers[i].SetActive(true);
            }
        }
    }





    public void SelectEncounter(int index)
    {
        SelectedEncounterIndex = index;
        NextEncounterUI.SetActive(false);
        battleManager.BeginBattleAfterEncounter();
    }





}
