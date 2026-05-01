using System;
using UnityEngine;

public class UnitStatsRuntime : MonoBehaviour
{
    [SerializeField] private Enemy enemyData;

    private float currentHealth;
    private float maxHealth;
    private Damage damage;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action<UnitStatsRuntime> OnDied;

    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError("UnitStatsRuntime: No Enemy data assigned.", this);
            return;
        }



        damage = GetComponent<Damage>();

        maxHealth = enemyData.GetHealth();
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public float ApplyDamage(float damageAmount)
    {
        Debug.Log($"Damage applied to: {gameObject.name}");

        damageAmount = Mathf.Max(0, damageAmount);
        currentHealth = damage.TakeDamage(currentHealth, maxHealth, damageAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if(currentHealth <= 0)
        {
            OnDied?.Invoke(this);
        }
        return currentHealth;
    }

    public Enemy GetEnemyData()
    {
        return enemyData;
    }
}