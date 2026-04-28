using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyUIHandler : MonoBehaviour
{
    [SerializeField] private UnitStatsRuntime statsRuntime;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI affinityText;
    [SerializeField] private GameObject EnemyAttackUI;
    [SerializeField] private BattleManager battleManager;

    private Enemy enemyData;
    public bool Continue = false;

    void Start()
    {
        if (statsRuntime == null)
        {
            Debug.LogError("EnemyUIHandler: No UnitStatsRuntime assigned!", this);
            return;
        }

        if (EnemyAttackUI != null)
        {
            EnemyAttackUI.SetActive(false);
        }

        enemyData = statsRuntime.GetEnemyData();

        if (enemyData == null)
        {
            Debug.LogError("EnemyUIHandler: UnitStatsRuntime has no Enemy data assigned!", this);
            return;
        }

        if (nameText != null)
        {
            nameText.text = enemyData.GetEnemyName();
        }

        if (affinityText != null)
        {
            affinityText.text = enemyData.GetAffinity()?.affinityName ?? "None";
        }

        statsRuntime.OnHealthChanged += RefreshUI;
        RefreshUI(statsRuntime.CurrentHealth, statsRuntime.MaxHealth);
    }

    private void OnDestroy()
    {
        if (statsRuntime != null)
        {
            statsRuntime.OnHealthChanged -= RefreshUI;
        }
    }

    private void RefreshUI(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        if (progressBar != null && maxHealth > 0)
        {
            progressBar.SetProgress(currentHealth / maxHealth);
        }
    }

    public void OnContinue()
    {
        Continue = true;

        if (battleManager != null)
        {
            battleManager.ContinueBattle();
        }

        HideEnemyAttackUI();
    }

    public void StartEnemyMessage(Move attack)
    {
        if (attack == null)
        {
            Debug.LogWarning("EnemyUIHandler: attack was null.");
            return;
        }

        string effectivenessMessage = "";
        if (battleManager != null)
        {
            effectivenessMessage = battleManager.LastEnemyEffectivenessMessage;
        }

        HandleEnemyAttackUI(enemyData.GetEnemyName(), attack.getMoveName(), effectivenessMessage);
    }

    private void HandleEnemyAttackUI(string enemyName, string attackName, string effectivenessMessage)
    {
        Continue = false;

        if (EnemyAttackUI != null)
        {
            EnemyAttackUI.SetActive(true);
        }

        var text = EnemyAttackUI.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.richText = true;

            string finalText = enemyName + " used " + attackName + "!";

            if (!string.IsNullOrEmpty(effectivenessMessage))
            {
                finalText += "\n" + effectivenessMessage;
            }

            text.text = finalText;
        }
    }

    private void HideEnemyAttackUI()
    {
        if (EnemyAttackUI != null)
        {
            EnemyAttackUI.SetActive(false);
        }
    }

    public void Bind(UnitStatsRuntime stats)
    {
        statsRuntime = stats;
        enemyData = stats.GetEnemyData();
        RefreshUI(stats.CurrentHealth, stats.MaxHealth);
    }
}