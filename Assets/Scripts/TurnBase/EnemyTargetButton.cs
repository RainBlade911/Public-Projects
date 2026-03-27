using UnityEngine;

public class EnemyTargetButton : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private BattleUnit targetEnemy;

    public void SelectTarget()
    {
        if (battleManager == null || targetEnemy == null) return;
        battleManager.OnPlayerAttackTarget(targetEnemy);
    }
}