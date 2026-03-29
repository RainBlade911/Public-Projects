using System;
using UnityEngine;

public class Damage : MonoBehaviour
{

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;
    public float TakeDamage(float currentHealth, float maxHealth, float damage)
    {
        currentHealth -= Mathf.Max(0, damage);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        return currentHealth;
    }
}
