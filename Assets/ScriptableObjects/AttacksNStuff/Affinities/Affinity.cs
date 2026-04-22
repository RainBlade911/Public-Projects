using UnityEngine;

[CreateAssetMenu(fileName = "Affinity", menuName = "Scriptable Objects/Affinity")]
public class Affinity : ScriptableObject
{
    public string affinityName;

    [Header("Relationships")]
    public Affinity[] StrongAgainst;
    public Affinity[] WeakAgainst;

    public bool IsStrongAgainst(Affinity other)
    {
        foreach(Affinity affinity in StrongAgainst)
        {
            if (affinity == other)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsWeakAgainst(Affinity other)
    {
        foreach (Affinity affinity in WeakAgainst)
        {
            if (affinity == other)
            {
                return true;
            }
        }
        return false;
    }

    public string GetAffinityName()
    {
        return affinityName;
    }
}
