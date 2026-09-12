using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; }

    public float currentHealth;
    public float currentMana;
    public float maxHealth;
    public float maxMana;
    public float speed;

    public List<Move> learnedMoves = new();
    public List<string> supportItems = new();

    // Added for boss reward persistence
    public float EOS = 1f;
    public float EOK = 1f;

    public bool hasSavedState = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveState(
        float savedCurrentHealth,
        float savedCurrentMana,
        float savedMaxHealth,
        float savedMaxMana,
        float savedSpeed,
        List<Move> savedMoves,
        List<string> savedItems,
        float savedEOS,
        float savedEOK)
    {
        currentHealth = savedCurrentHealth;
        currentMana = savedCurrentMana;
        maxHealth = savedMaxHealth;
        maxMana = savedMaxMana;
        speed = savedSpeed;

        learnedMoves = new List<Move>(savedMoves);
        supportItems = new List<string>(savedItems);

        // Save boss reward multipliers
        EOS = savedEOS;
        EOK = savedEOK;

        hasSavedState = true;

        Debug.Log(
            "PlayerState saved. " +
            "HP: " + currentHealth + "/" + maxHealth +
            ", Mana: " + currentMana + "/" + maxMana +
            ", Speed: " + speed +
            ", EOS: " + EOS +
            ", EOK: " + EOK +
            ", Moves: " + learnedMoves.Count +
            ", Items: " + supportItems.Count
        );
    }

    public void ClearState()
    {
        hasSavedState = false;
        learnedMoves.Clear();
        supportItems.Clear();

        // Reset boss rewards
        EOS = 1f;
        EOK = 1f;
    }
}