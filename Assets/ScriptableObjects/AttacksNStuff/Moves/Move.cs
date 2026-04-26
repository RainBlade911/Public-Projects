using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Scriptable Objects/Move")]
public class Move : ScriptableObject
{
    [SerializeField] private string moveName;
    [SerializeField] private float damage;
    [SerializeField] private Affinity type;
    [SerializeField] private float manaCost;
    [SerializeField] private ParticleSystem moveEffectPrefab;
    [SerializeField] private Vector3 offset;

    public string getMoveName()
    {
        return moveName;
    }

    public float getDamage()
    {
        return damage;
    }

    public float getManaCost()
    {
        return manaCost;
    }

    public Affinity getType()
    {
        return type;
    }

    public ParticleSystem getMoveEffectPrefab()
    {
        return moveEffectPrefab;
    }

    public Vector3 getOffset()
    {
        return offset;
    }
}
