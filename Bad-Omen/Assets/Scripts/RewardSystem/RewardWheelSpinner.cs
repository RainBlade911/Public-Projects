using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RewardWheelType
{
    Equipment,
    Moveset,
    Support
}

public class RewardWheelSpinner : MonoBehaviour
{
    [System.Serializable]
    public class WheelSlot
    {
        public string rewardName;
        public float centerAngle;
        public Color textColor = Color.white;
    }

    [Header("Wheel Root")]
    [SerializeField] private GameObject wheelUIRoot;
    [SerializeField] private CanvasGroup wheelCanvasGroup;

    [Header("Wheel Images")]
    [SerializeField] private RectTransform equipmentWheel;
    [SerializeField] private RectTransform movesetWheel;
    [SerializeField] private RectTransform supportWheel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI rewardNameText;

    [Header("Spin Timing")]
    [SerializeField] private float totalSpinTime = 3f;
    [SerializeField] private float resultReadDelay = 2f;
    [SerializeField] private int fullRotations = 6;

    [Header("Wheel Slot Data")]
    [SerializeField] private WheelSlot[] equipmentSlots =
    {
        new WheelSlot { rewardName = "Mana Reward", centerAngle = 0f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Health Reward", centerAngle = 50f, textColor = new Color(1f, 0.2f, 0.2f) },
        new WheelSlot { rewardName = "Mana Reward", centerAngle = 100f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Health Reward", centerAngle = 150f, textColor = new Color(1f, 0.2f, 0.2f) },
        new WheelSlot { rewardName = "Mana Reward", centerAngle = 200f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Combo Reward", centerAngle = 250f, textColor = new Color(0.75f, 0.35f, 1f) },
        new WheelSlot { rewardName = "Health Reward", centerAngle = 300f, textColor = new Color(1f, 0.2f, 0.2f) }
    };

    [SerializeField] private WheelSlot[] movesetSlots =
    {
        new WheelSlot { rewardName = "Static Electricity", centerAngle = 0f, textColor = new Color(1f, 1f, 0.25f) },
        new WheelSlot { rewardName = "Water Beam", centerAngle = 50f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Freeze", centerAngle = 100f, textColor = new Color(0.6f, 1f, 1f) },
        new WheelSlot { rewardName = "Spin Attack", centerAngle = 150f, textColor = Color.white },
        new WheelSlot { rewardName = "Breeze", centerAngle = 200f, textColor = new Color(0.5f, 1f, 0.8f) },
        new WheelSlot { rewardName = "Boulder", centerAngle = 250f, textColor = new Color(0.7f, 0.5f, 0.3f) }
    };

    [SerializeField] private WheelSlot[] supportSlots =
    {
        new WheelSlot { rewardName = "Mana Potion", centerAngle = 0f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Health Potion", centerAngle = 50f, textColor = new Color(1f, 0.2f, 0.2f) },
        new WheelSlot { rewardName = "Mana Potion", centerAngle = 100f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Health Potion", centerAngle = 150f, textColor = new Color(1f, 0.2f, 0.2f) },
        new WheelSlot { rewardName = "Mana Potion", centerAngle = 200f, textColor = new Color(0.4f, 0.8f, 1f) },
        new WheelSlot { rewardName = "Elixir Potion", centerAngle = 250f, textColor = new Color(0.75f, 0.35f, 1f) },
        new WheelSlot { rewardName = "Health Potion", centerAngle = 300f, textColor = new Color(1f, 0.2f, 0.2f) }
    };
/* Bug Fixing: Wheel does not show for first reward, but works for following rewards.
    private void Awake()
    {
        HideWheel();
    }
*/
    public IEnumerator PlayWheel(RewardWheelType wheelType, string finalRewardName)
    {
        RectTransform activeWheel = GetActiveWheel(wheelType);
        WheelSlot[] slots = GetSlots(wheelType);

        if (activeWheel == null)
        {
            Debug.LogWarning("RewardWheelSpinner: No wheel assigned for " + wheelType + ".");
            yield break;
        }

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("RewardWheelSpinner: No slots assigned for " + wheelType + ".");
            yield break;
        }

        ShowWheel(wheelType);

        WheelSlot finalSlot = FindSlot(slots, finalRewardName);
        float targetAngle = finalSlot.centerAngle;

        float startAngle = activeWheel.localEulerAngles.z;
        float endAngle = fullRotations * 360f + targetAngle;

        float timer = 0f;

        while (timer < totalSpinTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / totalSpinTime);

            float easedT = EaseOutCubic(t);
            float currentAngle = Mathf.Lerp(startAngle, endAngle, easedT);

            activeWheel.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

            WheelSlot currentSlot = GetClosestSlot(slots, currentAngle);
            UpdateRewardText(currentSlot.rewardName, currentSlot.textColor);

            yield return null;
        }

        activeWheel.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
        UpdateRewardText(finalSlot.rewardName, finalSlot.textColor);

        yield return new WaitForSeconds(resultReadDelay);

        HideWheel();
    }

    public void HideWheel()
    {
        if (wheelUIRoot != null)
            wheelUIRoot.SetActive(false);

        if (wheelCanvasGroup != null)
        {
            wheelCanvasGroup.alpha = 0f;
            wheelCanvasGroup.interactable = false;
            wheelCanvasGroup.blocksRaycasts = false;
        }
    }

    private void ShowWheel(RewardWheelType wheelType)
    {
        if (wheelUIRoot != null)
        {
            wheelUIRoot.SetActive(true);
            wheelUIRoot.transform.SetAsLastSibling();
        }

        if (wheelCanvasGroup != null)
        {
            wheelCanvasGroup.alpha = 1f;
            wheelCanvasGroup.interactable = false;
            wheelCanvasGroup.blocksRaycasts = true;
        }

        if (equipmentWheel != null)
            equipmentWheel.gameObject.SetActive(wheelType == RewardWheelType.Equipment);

        if (movesetWheel != null)
            movesetWheel.gameObject.SetActive(wheelType == RewardWheelType.Moveset);

        if (supportWheel != null)
            supportWheel.gameObject.SetActive(wheelType == RewardWheelType.Support);
    }

    private RectTransform GetActiveWheel(RewardWheelType wheelType)
    {
        switch (wheelType)
        {
            case RewardWheelType.Equipment:
                return equipmentWheel;

            case RewardWheelType.Moveset:
                return movesetWheel;

            case RewardWheelType.Support:
                return supportWheel;

            default:
                return null;
        }
    }

    private WheelSlot[] GetSlots(RewardWheelType wheelType)
    {
        switch (wheelType)
        {
            case RewardWheelType.Equipment:
                return equipmentSlots;

            case RewardWheelType.Moveset:
                return movesetSlots;

            case RewardWheelType.Support:
                return supportSlots;

            default:
                return null;
        }
    }

    private WheelSlot FindSlot(WheelSlot[] slots, string rewardName)
    {
        foreach (WheelSlot slot in slots)
        {
            if (slot.rewardName == rewardName)
                return slot;
        }

        Debug.LogWarning("RewardWheelSpinner: Could not find slot for " + rewardName + ". Using first slot.");
        return slots[0];
    }

    private WheelSlot GetClosestSlot(WheelSlot[] slots, float angle)
    {
        float normalizedAngle = NormalizeAngle(angle);
        WheelSlot closestSlot = slots[0];
        float closestDistance = 999f;

        foreach (WheelSlot slot in slots)
        {
            float distance = Mathf.Abs(Mathf.DeltaAngle(normalizedAngle, slot.centerAngle));

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }

        return closestSlot;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
            angle += 360f;

        return angle;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private void UpdateRewardText(string text, Color color)
    {
        if (rewardNameText == null)
            return;

        rewardNameText.text = text;
        rewardNameText.color = color;
    }
}