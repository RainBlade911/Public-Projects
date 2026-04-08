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
        actionText.text = text;
    }

    public void SetTextActive()
    {
        actionTextContainer.SetActive(true);
    }

    public void HideText()
    {
        actionTextContainer.SetActive(false);
    }
}
