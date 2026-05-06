using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyEncounters : MonoBehaviour
{
    [SerializeField] private List<GameObject> EnemyInfo = new List<GameObject>();

    public void SetEnemyInfo(int index, string enemyName, string affinity, Sprite enemyIcon, bool isBoss = false)
{
    if (EnemyInfo[index] == null)
    {
        Debug.LogError($"EnemyInfo[{index}] is NULL on {name}");
        return;
    }

    Transform stuff = EnemyInfo[index].transform;

    var nameObj = stuff.Find("EnemyName");
    var affinityObj = stuff.Find("Affinity");
    var iconObj = stuff.Find("EnemyIcon");

    if (nameObj == null)
        Debug.LogError($"EnemyName NOT FOUND under {EnemyInfo[index].name}");

    if (affinityObj == null)
        Debug.LogError($"Affinity NOT FOUND under {EnemyInfo[index].name}");

    if (iconObj == null)
        Debug.LogError($"EnemyIcon NOT FOUND under {EnemyInfo[index].name}");

    if (nameObj && affinityObj && iconObj)
    {
        var nameText = nameObj.GetComponent<TextMeshProUGUI>();
        var affinityText = affinityObj.GetComponent<TextMeshProUGUI>();

        nameText.text = enemyName;
        affinityText.text = affinity;

        iconObj.GetComponent<Image>().sprite = enemyIcon;

        // Added: boss visual distinction
        if (isBoss)
        {
            nameText.color = Color.red;
            affinityText.color = Color.red;

            nameText.text = enemyName;
        }
        else
        {
            nameText.color = Color.white;
            affinityText.color = Color.white;
        }
    }
}

    public void HideUnusedSlots(int enemyCount)
    {
        for (int i = 0; i < EnemyInfo.Count; i++)
        {
            EnemyInfo[i].SetActive(i < enemyCount);
        }
    }

}

