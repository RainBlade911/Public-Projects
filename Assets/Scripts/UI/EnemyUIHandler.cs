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

        EnemyAttackUI.SetActive(false);

        

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
            Debug.Log("Should Update Progress Bar: " + (float)currentHealth / maxHealth);
            progressBar.SetProgress((float)currentHealth / maxHealth);
        }
    }

    public void OnContinue()
    {
        Continue = true;
        battleManager.ContinueBattle();
        HideEnemyAttackUI();
    }

    public void StartEnemyMessage(Move attack)
    {
        HandleEnemyAttackUI(enemyData.GetEnemyName(), attack.getMoveName(), attack.getDamage());
    }
    private void HandleEnemyAttackUI(string EnemyName, string attack, float damage)
    {
        Continue = false;
        EnemyAttackUI.SetActive(true);
        var text = EnemyAttackUI.GetComponentInChildren<TextMeshProUGUI>();
        text.text = EnemyName + " is attacking with " + attack + "!\nIt deals " + damage + " damage!"; 
    }

    private void HideEnemyAttackUI()
    {
        EnemyAttackUI.SetActive(false);
    }
}