using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{
    [SerializeField] private string EnemyName;
    [SerializeField] private int Health;
    [SerializeField] private MoveSet moves;
    [SerializeField] Affinity affinity;


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
}
