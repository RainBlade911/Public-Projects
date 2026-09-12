using System;
using UnityEngine;
using UnityEngine.InputSystem;   // IMPORTANT

public class RaycastSelectionManager : MonoBehaviour
{
    private EnemySelection hoveredEnemy;
    private EnemySelection selectedEnemy;

    void Update()
    {
        HandleHover();
        HandleClick();
    }

    private void HandleHover()
    {
        // NEW INPUT SYSTEM: get mouse position
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemySelection enemy = hit.collider.GetComponent<EnemySelection>();

            if (enemy != hoveredEnemy)
            {
                // Remove highlight from previous hovered enemy
                if (hoveredEnemy != null && hoveredEnemy != selectedEnemy)
                    hoveredEnemy.SetHighlighted(false);

                hoveredEnemy = enemy;

                // Highlight new hovered enemy (if not selected)
                if (hoveredEnemy != null && hoveredEnemy != selectedEnemy)
                    hoveredEnemy.SetHighlighted(true);
            }
        }
        else
        {
            // No hit — remove hover highlight
            if (hoveredEnemy != null && hoveredEnemy != selectedEnemy)
                hoveredEnemy.SetHighlighted(false);

            hoveredEnemy = null;
        }
    }

    private void HandleClick()
    {
        // NEW INPUT SYSTEM: left mouse click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hoveredEnemy != null)
            {
                // Remove highlight from previously selected enemy
                if (selectedEnemy != null)
                    selectedEnemy.SetHighlighted(false);

                selectedEnemy = hoveredEnemy;

                // Selected enemy stays highlighted
                selectedEnemy.SetHighlighted(true);


                Debug.Log("Selected enemy: " + selectedEnemy.name);
            }
        }
    }

    public BattleUnit GetSelectedEnemy()
    {
        return selectedEnemy != null ? selectedEnemy.GetBattleUnit() : null;
    }

    internal void KeepSelected(BattleUnit enemy)
    {
        selectedEnemy = enemy.GetComponent<EnemySelection>();
        enemy.GetComponent<EnemySelection>().Disable();
    }
}
