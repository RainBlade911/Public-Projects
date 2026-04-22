using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fillText;
    [SerializeField] private Image fillImage;

    private float fill = 1f;

    public float Fill
    {
        get { return fill; }
        set
        {
            fill = Mathf.Clamp01(value);

            if (fillImage != null)
            {
                fillImage.fillAmount = fill;
            }

            
        }
    }

    public void SetProgress(float value)
    {
        Fill = value;
    }

    public void SetProgressText(string text)
    {
        if (fillText != null)
        {
            fillText.text = text;
        }
    }
}