using NUnit.Framework;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Move[] moves;
    private float CurrentHealth;
    private float MaxHealth = 20f;
    private float CurrentMana;
    private float MaxMana = 10f;
    private float BaseSpeed = 5f;
    private float CurrentSpeed;

    [SerializeField] PlayerManaBar manaBar;
    [SerializeField] PlayerHealthBar healthBar;

    [SerializeField] MoveSet defaultMoves;
    [SerializeField] Sprite playerSprite;

    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize stats early so other scripts can read them
        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;
        CurrentSpeed = BaseSpeed;
    }


    private void Start()
    {
        moves = defaultMoves.GetMoves();
        
    }

    public Move GetMove(int index)
    {
        if (index < 0 || index >= moves.Length)
        {
            Debug.LogError("Invalid move index: " + index);
            return null;
        }
        return moves[index];
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }

    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    public float GetCurrentMana()
    {
        return CurrentMana;
    }

    public float GetMaxMana()
    {
        return MaxMana;
    }

    public float ApplyManaCost(float manaCost)
    {
        manaCost = Mathf.Max(0, manaCost);
        CurrentMana -= manaCost;
        CurrentMana = Mathf.Clamp(CurrentMana, 0, MaxMana);
        manaBar.SetProgress(CurrentMana / MaxMana);
        return CurrentMana;
    }

    public float getCurrentSpeed()
    {
        Debug.Log("PlayerManager CurrentSpeed = " + CurrentSpeed);

        return CurrentSpeed;
    }

    public float ApplyDamage(float damage)
    {
        damage = Mathf.Max(0, damage);
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        healthBar.SetProgress(CurrentHealth / MaxHealth);
        return CurrentHealth;
    }

    public Sprite GetSprite()
    {
        return playerSprite;
    }

}
