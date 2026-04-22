using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Moves")]
    [SerializeField] public List<Move> moves = new();
    [SerializeField] private List<Move> defaultMoves = new();

    [Header("Support Items")]
    [SerializeField] private List<string> supportItems = new();

    [Header("UI")]
    [SerializeField] private ProgressBar manaBar;
    [SerializeField] private ProgressBar healthBar;

    [Header("Player Display")]
    [SerializeField] private Sprite playerSprite;

    [Header("Affinity")]
    [SerializeField] private Affinity defaultAffinity;
    [SerializeField] private Affinity currentAffinity;

    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float currentHealth = 10f;
    [SerializeField] private float maxMana = 10f;
    [SerializeField] private float currentMana = 10f;
    [SerializeField] private float speed = 5f;

    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        if (initialized) return;

        if (PlayerState.Instance != null && PlayerState.Instance.hasSavedState)
        {
            maxHealth = PlayerState.Instance.maxHealth;
            maxMana = PlayerState.Instance.maxMana;
            speed = PlayerState.Instance.speed;

            currentHealth = Mathf.Clamp(PlayerState.Instance.currentHealth, 0f, maxHealth);
            currentMana = Mathf.Clamp(PlayerState.Instance.currentMana, 0f, maxMana);

            moves = new List<Move>(PlayerState.Instance.learnedMoves);
            supportItems = new List<string>(PlayerState.Instance.supportItems);

            Debug.Log(
                "PlayerManager: Restored saved state. " +
                "HP: " + currentHealth + "/" + maxHealth +
                ", Mana: " + currentMana + "/" + maxMana +
                ", Speed: " + speed +
                ", Moves: " + moves.Count +
                ", Items: " + supportItems.Count
            );
        }
        else
        {
            currentHealth = maxHealth;
            currentMana = maxMana;

            moves = new List<Move>();
            if (defaultMoves != null && defaultMoves.Count > 0)
            {
                moves.AddRange(defaultMoves);
            }

            supportItems = new List<string>();

            Debug.Log(
                "PlayerManager: Initialized fresh state. " +
                "HP: " + currentHealth + "/" + maxHealth +
                ", Mana: " + currentMana + "/" + maxMana +
                ", Moves: " + moves.Count
            );
        }

        currentAffinity = defaultAffinity;
        Debug.Log("Player affinity reset to: " + GetAffinityName());

        UpdateBars();
        initialized = true;
    }

    public float ApplyDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateBars();
        Debug.Log("Player took " + damage + " damage. Remaining HP: " + currentHealth);

        return currentHealth;
    }

    public float ApplyManaCost(float manaCost)
    {
        currentMana -= manaCost;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        UpdateBars();
        Debug.Log("Player used mana. Remaining Mana: " + currentMana);

        return currentMana;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateBars();
        Debug.Log("Player healed. Current HP: " + currentHealth);
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        UpdateBars();
        Debug.Log("Player restored mana. Current Mana: " + currentMana);
    }

    public Move GetMove(int index)
    {
        if (index < 0 || index >= moves.Count)
        {
            Debug.LogWarning("PlayerManager: Invalid move index " + index);
            return null;
        }

        return moves[index];
    }

    public List<Move> GetMoves()
    {
        return moves;
    }

    public void AddMove(Move move)
    {
        if (move == null)
        {
            Debug.LogWarning("PlayerManager: Tried to add a null move.");
            return;
        }

        if (!moves.Contains(move))
        {
            moves.Add(move);
            Debug.Log("PlayerManager: Added move " + move.getMoveName());
        }
        else
        {
            Debug.Log("PlayerManager: Move already owned: " + move.getMoveName());
        }
    }

    public void AddItem(string itemName)
    {
        supportItems.Add(itemName);
        Debug.Log("PlayerManager: Added item " + itemName + ". Total items: " + supportItems.Count);
    }

    public List<string> GetItems()
    {
        return supportItems;
    }

    public bool UseItem(int index)
    {
        if (index < 0 || index >= supportItems.Count)
        {
            Debug.LogWarning("PlayerManager: Invalid item index " + index);
            return false;
        }

        string itemName = supportItems[index];

        switch (itemName)
        {
            case "Health Potion":
                Heal(10f);
                Debug.Log("Player used Health Potion and restored 10 HP.");
                break;

            case "Mana Potion":
                RestoreMana(10f);
                Debug.Log("Player used Mana Potion and restored 10 Mana.");
                break;

            default:
                Debug.Log("Player used " + itemName + ".");
                break;
        }

        supportItems.RemoveAt(index);
        return true;
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateBars();
        Debug.Log("PlayerManager: Max health increased by " + amount + ". New Max HP: " + maxHealth);
    }

    public void IncreaseMaxMana(float amount)
    {
        maxMana += amount;
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

        UpdateBars();
        Debug.Log("PlayerManager: Max mana increased by " + amount + ". New Max Mana: " + maxMana);
    }

    public void IncreaseSpeed(float amount)
    {
        speed += amount;
        Debug.Log("PlayerManager: Speed increased by " + amount + ". New Speed: " + speed);
    }

    public void SetAffinity(Affinity newAffinity)
    {
        currentAffinity = newAffinity;
        Debug.Log("Player affinity changed to: " + GetAffinityName());
    }

    public Affinity GetAffinity()
    {
        return currentAffinity;
    }

    public string GetAffinityName()
    {
        return currentAffinity != null ? currentAffinity.affinityName : "None";
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentMana()
    {
        return currentMana;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetMaxMana()
    {
        return maxMana;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float getCurrentSpeed()
    {
        return speed;
    }

    public Sprite GetPlayerSprite()
    {
        return playerSprite;
    }

    public Sprite GetSprite()
    {
        return playerSprite;
    }

    public void SavePersistentState()
    {
        if (PlayerState.Instance == null) return;

        PlayerState.Instance.SaveState(
            currentHealth,
            currentMana,
            maxHealth,
            maxMana,
            speed,
            moves,
            supportItems
        );
    }

    private void UpdateBars()
    {
        if (healthBar != null)
        {
            healthBar.Fill = maxHealth <= 0 ? 0f : currentHealth / maxHealth;
        }

        if (manaBar != null)
        {
            manaBar.Fill = maxMana <= 0 ? 0f : currentMana / maxMana;
        }
    }
}