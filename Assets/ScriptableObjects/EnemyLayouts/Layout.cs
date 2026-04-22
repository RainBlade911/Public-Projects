using UnityEngine;

[CreateAssetMenu(fileName = "Layout", menuName = "Scriptable Objects/Layout")]
public class Layout : ScriptableObject
{
    private const int MaxEnemies = 3;

    [SerializeField] private BattleUnit[] enemies = new BattleUnit[MaxEnemies];

    public Layout GetLayout()
    {
        return this;
    }
}
