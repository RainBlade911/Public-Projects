using UnityEngine;

public class EnemyTurnAI : MonoBehaviour
{
    [SerializeField] private BattleUnit owner;
    //expand later with random selection, special moves, moves with affinity, status effects, etc
    public int GetAttackDamage()
    {
        if (owner == null || owner.Stats == null)
        {
            return 0;
        }

        return owner.Stats.AttackPower;
    }
}