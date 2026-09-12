using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EnemySelection : MonoBehaviour
{
    private BattleUnit battleUnit;
    private GameObject outlineObject;
    private bool enabled = true;

    private void Awake()
    {
        battleUnit = GetComponent<BattleUnit>();


        // Find the outline mesh in children
        outlineObject = transform.Find("ArrowPointer")?.gameObject;

        if (outlineObject == null)
        {
            Debug.LogWarning($"EnemySelection: No ArrowPointer found on {gameObject.name}");
        }
        else
        {
            outlineObject.SetActive(false); // start disabled
        }
    }

    public BattleUnit GetBattleUnit()
    {
        return battleUnit;
    }

    public void SetHighlighted(bool value)
    {
        if (outlineObject != null && enabled)
            outlineObject.SetActive(value);
    }

    internal void Disable()
    {
        enabled = false;
        if (outlineObject != null)
            outlineObject.SetActive(false);

    }
}
