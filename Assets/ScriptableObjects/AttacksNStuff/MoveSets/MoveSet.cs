using UnityEngine;

[CreateAssetMenu(fileName = "MoveSets", menuName = "Scriptable Objects/MoveSets")]
public class MoveSet : ScriptableObject
{
    [SerializeField] private Move[] moves;
}
