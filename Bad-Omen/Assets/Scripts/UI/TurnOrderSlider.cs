using UnityEngine;
using UnityEngine.UI;

public class TurnOrderSlider : MonoBehaviour
{
    [SerializeField] private Image CurrentTurn;
    [SerializeField] private Image Turn2;
    [SerializeField] private Image Turn3;
    [SerializeField] private Image Turn4;

    public void UpdateTurnOrder(Sprite[] turnOrder)
    {
        Image[] slots = { CurrentTurn, Turn2, Turn3, Turn4 };

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < turnOrder.Length && turnOrder[i] != null)
            {
                slots[i].enabled = true;
                slots[i].sprite = turnOrder[i];
            }
            else
            {
                slots[i].enabled = false; // hides the white box
            }
        }
    }

}
