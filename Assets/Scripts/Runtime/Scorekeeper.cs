using UnityEngine;

public class Scorekeeper : MonoBehaviour
{

    private float FinalScore = 0;

    private int currentScore = 0;

    private int RoundCounter = -1;

    public static Scorekeeper Instance;

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

    public void addScore(int score)
    {
        Debug.Log($"Score added: {score}. Current Score: {currentScore} -> {currentScore + score}");
        currentScore += score;
    }

    public void CalculateFinalScore()
    {
        FinalScore = currentScore * (1 + (RoundCounter/4f));
        Debug.Log($"FINAL SCORE: {FinalScore} (Base Score: {currentScore}, Round Multiplier: {(1 + (RoundCounter/4f))})");
    }

    public float GetFinalScore()
    {
        return FinalScore;
    }

    public void IncrementRoundCounter()
    {
        RoundCounter++;
    }

    public void ResetScore()
    {
        currentScore = 0;
        RoundCounter = -1;
        FinalScore = 0;
    }

    
}
