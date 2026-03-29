using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private UnitStatsRuntime currentEnemy;

    private void Start()
    {
        StartBattle();
    }

    private void StartBattle()
    {

        if (currentEnemy == null)
        {
            Debug.LogError("BattleManager: No current enemy assigned.", this);
            return;
        }

        currentEnemy.OnDied += HandleEnemyDied;
    }

    private void OnDestroy()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnDied -= HandleEnemyDied;
        }
    }

    public void AttackSelected(Move move)
    {

        if (currentEnemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy to attack.");
            return;
        }

        Debug.Log("Performing Attack: " + move.name);


        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }

        int remainingHealth = currentEnemy.ApplyDamage(5);
        Debug.Log("Enemy took 5 damage. Remaining HP: " + remainingHealth);
    }

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        Debug.Log("Enemy defeated.");

        if (deadEnemy != null)
        {
            Destroy(deadEnemy.gameObject);
        }

        currentEnemy = null;
    }

    public Move getMove(int i)
    {
        return PlayerManager.Instance.GetMove(i);
    }
}