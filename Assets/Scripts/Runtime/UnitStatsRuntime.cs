using System;
using UnityEngine;

public class UnitStatsRuntime : MonoBehaviour
{
    [SerializeField] private Enemy enemyData;

    private int currentHealth;
    private int maxHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> OnHealthChanged;

    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("UnitStatsRuntime: No Enemy data assigned.", this);
            return;
        }

        maxHealth = enemyData.GetHealth();
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        ApplyDamage(5);
    }

    public int ApplyDamage(int damageAmount)
    {
        damageAmount = Mathf.Max(0, damageAmount);

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        return currentHealth;
    }

    public Enemy GetEnemyData()
    {
        return enemyData;
    }
}