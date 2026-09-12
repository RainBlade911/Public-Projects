using UnityEngine;

[CreateAssetMenu(fileName = "MoveSets", menuName = "Scriptable Objects/MoveSets")]
public class MoveSet : ScriptableObject
{
    [SerializeField] private Move[] moves;

    public Move[] GetMoves()
    {
        return moves;
    }
}
