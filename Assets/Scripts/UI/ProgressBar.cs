using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image Fill;

    public void SetProgress(float value)
    {
        if (Fill == null) return;
        Fill.fillAmount = Mathf.Clamp01(value);
    }

    //void Start()
    //{
    //    SetProgress(1f);
    //}
}