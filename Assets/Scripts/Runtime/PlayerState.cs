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
        List<string> savedItems)
    {
        currentHealth = savedCurrentHealth;
        currentMana = savedCurrentMana;
        maxHealth = savedMaxHealth;
        maxMana = savedMaxMana;
        speed = savedSpeed;

        learnedMoves = new List<Move>(savedMoves);
        supportItems = new List<string>(savedItems);

        hasSavedState = true;

        Debug.Log(
            "PlayerState saved. " +
            "HP: " + currentHealth + "/" + maxHealth +
            ", Mana: " + currentMana + "/" + maxMana +
            ", Speed: " + speed +
            ", Moves: " + learnedMoves.Count +
            ", Items: " + supportItems.Count
        );
    }

    public void ClearState()
    {
        hasSavedState = false;
        learnedMoves.Clear();
        supportItems.Clear();
    }
}