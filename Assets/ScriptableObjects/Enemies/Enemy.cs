using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{
    [SerializeField] private string EnemyName;
    [SerializeField] private int Health;
    [SerializeField] private MoveSet moves;
    [SerializeField] Affinity affinity;
    [SerializeField] private float speed;
    [SerializeField] private Sprite enemySprite;

    public string GetEnemyName()
    {
        return EnemyName;
    }

    public int GetHealth()
    {
        return Health;
    }

    public Affinity GetAffinity()
    {
        return affinity;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public Move[] GetMoves()
    {
        return moves.GetMoves();
    }

    public Move GetRandomMove()
    {
        Move[] availableMoves = GetMoves();
        if (availableMoves.Length == 0)
        {
            Debug.LogWarning("Enemy " + EnemyName + " has no moves defined!");
            return null;
        }
        int randomIndex = Random.Range(0, availableMoves.Length);
        return availableMoves[randomIndex];
    }

    public Sprite GetSprite()
    {
        return enemySprite;
    }



}
