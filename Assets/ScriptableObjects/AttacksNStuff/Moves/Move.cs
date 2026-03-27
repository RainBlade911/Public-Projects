using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Scriptable Objects/Move")]
public class Move : ScriptableObject
{
    [SerializeField] private string moveName;
    [SerializeField] private float damage;
    [SerializeField] private Affinity type;
    [SerializeField] private float manaCost;

    private string getMoveName()
    {
        return moveName;
    }

    private float getDamage()
    {
        return damage;
    }

    private Affinity getType()
    {
        return type;
    }
}
