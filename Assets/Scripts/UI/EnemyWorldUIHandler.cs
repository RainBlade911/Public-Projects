using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyWorldUIHandler : MonoBehaviour
{
    [SerializeField] private UnitStatsRuntime statsRuntime;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI affinityText;

    private Enemy enemyData;

    private void Start()
    {
        if (statsRuntime == null)
            return;

        enemyData = statsRuntime.GetEnemyData();

        if (nameText != null)
            nameText.text = enemyData.GetEnemyName();

        if (affinityText != null)
            affinityText.text = enemyData.GetAffinity()?.affinityName ?? "None";


        RefreshUI(statsRuntime.CurrentHealth, statsRuntime.MaxHealth);
    }

    private void OnDestroy()
    {
        if (statsRuntime != null)
            statsRuntime.OnHealthChanged -= RefreshUI;
    }

    private void RefreshUI(float currentHealth, float maxHealth)
    {
        if (healthText != null)
            healthText.text = $"{currentHealth}/{maxHealth}";

        if (progressBar != null && maxHealth > 0)
            progressBar.SetProgress(currentHealth / maxHealth);
    }

    public void Bind(UnitStatsRuntime stats)
    {
        // Unsubscribe from old stats if needed
        if (statsRuntime != null)
            statsRuntime.OnHealthChanged -= RefreshUI;

        statsRuntime = stats;
        enemyData = stats.GetEnemyData();

        // Subscribe to new stats
        statsRuntime.OnHealthChanged += RefreshUI;

        // Update immediately
        RefreshUI(stats.CurrentHealth, stats.MaxHealth);
    }
}
