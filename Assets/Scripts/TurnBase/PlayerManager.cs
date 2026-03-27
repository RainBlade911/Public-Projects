using NUnit.Framework;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Move[] moves;

    [SerializeField] MoveSet defaultMoves;

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

}
