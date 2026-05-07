using System.Collections.Generic;
using TMPro;
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

    [Header("Boss Reward Damage Multipliers")]
    [SerializeField] private float EOS = 1f;
    [SerializeField] private float EOK = 1f;

    [Header("Battle Unit Reference")]
    [SerializeField] private BattleUnit playerUnit;

    private bool initialized = false;

    [SerializeField] private MoveSet moveSet; // ScriptableObject containing moves

    [SerializeField] TextMeshProUGUI AffinityText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        moves = new List<Move>(moveSet.GetMoves());
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

            EOS = PlayerState.Instance.EOS;
            EOK = PlayerState.Instance.EOK;

            Debug.Log(
                "PlayerManager: Restored saved state. " +
                "HP: " + currentHealth + "/" + maxHealth +
                ", Mana: " + currentMana + "/" + maxMana +
                ", Speed: " + speed +
                ", EOS: " + EOS +
                ", EOK: " + EOK +
                ", Moves: " + moves.Count +
                ", Items: " + supportItems.Count
            );
        }
        else
        {
            currentHealth = maxHealth;
            currentMana = maxMana;

            moves = new List<Move>(moveSet.GetMoves());

            supportItems = new List<string>();

            EOS = 1f;
            EOK = 1f;

            Debug.Log(
                "PlayerManager: Initialized fresh state. " +
                "HP: " + currentHealth + "/" + maxHealth +
                ", Mana: " + currentMana + "/" + maxMana +
                ", Moves: " + moves.Count
            );
        }

        currentAffinity = defaultAffinity;
        Debug.Log("Player affinity reset to: " + GetAffinityName());

        AffinityText.text = currentAffinity.GetAffinityName();

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

    public List<Move> GetMoves() => moves;

    public Move GetMove(int index)
    {
        if (index < 0 || index >= moves.Count)
            return null;
        return moves[index];
    }

    public void AddMove(Move move)
    {
        if (!moves.Contains(move))
            moves.Add(move);
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

    public void UpgradeEOS()
    {
        EOS += 0.1f;
        Debug.Log("Essence of Strength upgraded. New EOS multiplier: " + EOS);
    }

    public void UpgradeEOK()
    {
        EOK += 0.05f;
        Debug.Log("Essence of Knowledge upgraded. New EOK multiplier: " + EOK);
    }

    public void SetEOS(float value)
    {
        EOS = Mathf.Max(1f, value);
    }

    public void SetEOK(float value)
    {
        EOK = Mathf.Max(1f, value);
    }

    public float GetEOS()
    {
        return EOS;
    }

    public float GetEOK()
    {
        return EOK;
    }

    public float GetDamageMultiplierForMove(Move move)
    {
        if (move == null)
            return 1f;

        // EOS only applies to Strike.
        // EOK only applies to non-Strike moves.
        // These are intentionally not multiplied together.
        if (move.getMoveName() == "Strike")
            return EOS;

        return EOK;
    }

    public void SetAffinity(Affinity newAffinity)
    {
        currentAffinity = newAffinity;
        Debug.Log("Player affinity changed to: " + GetAffinityName());

        AffinityText.text = currentAffinity.GetAffinityName();
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
            supportItems,
            EOS,
            EOK
        );
    }

    private void UpdateBars()
    {
        if (healthBar != null)
        {
            healthBar.Fill = maxHealth <= 0 ? 0f : currentHealth / maxHealth;
            healthBar.SetProgressText($"{currentHealth}/{maxHealth}");
        }

        if (manaBar != null)
        {
            manaBar.Fill = maxMana <= 0 ? 0f : currentMana / maxMana;
            manaBar.SetProgressText($"{currentMana}/{maxMana}");
        }
    }

    // Added from EnemySpawns
    public BattleUnit GetBattleUnit()
    {
        return playerUnit;
    }
}