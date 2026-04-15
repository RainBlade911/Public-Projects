using TMPro;
using UnityEngine;

public class SupportItemButton : ButtonScript
{
    [SerializeField] private TextMeshProUGUI itemLabel;

    private int itemIndex;

    public void Initialize(int index, string itemName)
    {
        itemIndex = index;
        itemLabel.text = itemName;
    }

    public void OnClick()
    {
        if (itemLabel == null)
        {
            Debug.LogWarning("SupportItemButton: Item label is missing.");
            playerMenus.ChangeUITo("Default");
            return;
        }

        if (itemLabel.text == "Empty")
        {
            Debug.Log("No item in slot " + (itemIndex + 1));
            playerMenus.ChangeUITo("Default");
            return;
        }

        bool used = PlayerManager.Instance.UseItem(itemIndex);

        if (!used)
        {
            Debug.Log("No item in slot " + (itemIndex + 1));
            playerMenus.ChangeUITo("Default");
            return;
        }

        SetActionMessage("Player used " + itemLabel.text + "!");
        playerMenus.ChangeUITo("Default");
    }
}