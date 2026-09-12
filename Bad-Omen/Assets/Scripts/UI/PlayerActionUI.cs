using TMPro;
using UnityEngine;

public class PlayerActionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject actionTextContainer;

    private void Start()
    {
        HideText();
    }

    public void SetActionText(string text)
    {
        if (actionText != null)
        {
            actionText.richText = true;
            actionText.text = text;
        }
    }

    public void SetTextActive()
    {
        if (actionTextContainer != null)
        {
            actionTextContainer.SetActive(true);
        }
    }

    public void HideText()
    {
        if (actionTextContainer != null)
        {
            actionTextContainer.SetActive(false);
        }
    }
}