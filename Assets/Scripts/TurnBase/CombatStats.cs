using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [SerializeField] private int attackPower = 10;

    public int AttackPower => attackPower;
    //Expand later with other stats (defense,magic,mana,etc)
}