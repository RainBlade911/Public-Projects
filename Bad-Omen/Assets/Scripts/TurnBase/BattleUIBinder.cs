//using TMPro;
//using UnityEngine;

//public class BattleUIBinder : MonoBehaviour
//{
//    [SerializeField] private BattleUnit unit;
//    [SerializeField] private TextMeshProUGUI nameText;
//    [SerializeField] private TextMeshProUGUI healthText;
//    [SerializeField] private ProgressBar progressBar;

//    private void Start()
//    {
//        if (unit == null)
//        {
//            Debug.LogError("BattleUIBinder: No BattleUnit assigned.", this);
//            return;
//        }

//        if (unit.Health == null)
//        {
//            Debug.LogError("BattleUIBinder: BattleUnit has no Health component.", this);
//            return;
//        }

//        if (nameText != null)
//        {
//            nameText.text = unit.UnitName;
//        }

//        unit.Health.OnHealthChanged += RefreshUI;
//        RefreshUI(unit.Health.CurrentHealth, unit.Health.MaxHealth);
//    }

//    private void OnDestroy()
//    {
//        if (unit != null && unit.Health != null)
//        {
//            unit.Health.OnHealthChanged -= RefreshUI;
//        }
//    }

//    private void RefreshUI(int currentHealth, int maxHealth)
//    {
//        if (healthText != null)
//        {
//            healthText.text = $"{currentHealth}/{maxHealth}";
//        }

//        if (progressBar != null && maxHealth > 0)
//        {
//            progressBar.SetProgress((float)currentHealth / maxHealth);
//        }
//    }
//}