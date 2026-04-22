using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private UnitStatsRuntime currentEnemy;
    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private RewardController rewardController;
    [SerializeField] private float rewardDelay = 1.5f;
    [SerializeField] private PlayerActionUI playerActionUI;

    private bool cont = false;
    private bool battleEnded = false;

    public Move enemyMove;
    public string LastEnemyEffectivenessMessage { get; private set; } = "";

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

        if (rewardController != null)
        {
            rewardController.HideRewardUI();
        }

        battleEnded = false;
        LastEnemyEffectivenessMessage = "";
        turnController.OnBattleStart();
        currentEnemy.OnDied += HandleEnemyDied;
    }

    private void OnDestroy()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnDied -= HandleEnemyDied;
        }
    }

    public Move getMove(int i)
    {
        return PlayerManager.Instance.GetMove(i);
    }

    public void AttackSelected(Move move)
    {
        if (battleEnded) return;

        if (move == null)
        {
            Debug.LogWarning("BattleManager: Selected move was null.");
            return;
        }

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

        float baseDamage = move.getDamage();

        Affinity moveAffinity = move.getType();
        Affinity enemyAffinity = currentEnemy.GetEnemyData().GetAffinity();

        string effectivenessMessage;
        float multiplier = GetAffinityMultiplier(moveAffinity, enemyAffinity, out effectivenessMessage);
        float finalDamage = baseDamage * multiplier;

        float remainingHealth = currentEnemy.ApplyDamage(finalDamage);
        float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());

        PlayerManager.Instance.SetAffinity(moveAffinity);

        if (playerActionUI != null)
        {
            string finalText = "Player used " + move.getMoveName() + "!";
            if (!string.IsNullOrEmpty(effectivenessMessage))
            {
                finalText += "\n" + effectivenessMessage;
            }

            playerActionUI.SetActionText(finalText);
            playerActionUI.SetTextActive();
        }

        Debug.Log(
            "Player used " + move.getMoveName() +
            " | Base: " + baseDamage +
            " | Multiplier: " + multiplier +
            " | Final: " + finalDamage
        );

        Debug.Log(
            "Enemy took " + finalDamage +
            " damage. Remaining HP: " + remainingHealth +
            ". Player remaining mana: " + remainingMana
        );
    }

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        if (battleEnded) return;

        battleEnded = true;
        StartCoroutine(HandleEnemyDefeatRoutine(deadEnemy));
    }

    private IEnumerator HandleEnemyDefeatRoutine(UnitStatsRuntime deadEnemy)
    {
        Debug.Log("Enemy defeated.");

        turnController.StopBattleFlow();

        if (ParticlePlayer.Instance != null)
        {
            ParticlePlayer.Instance.PlayParticleEffect();
        }

        yield return new WaitForSeconds(rewardDelay);

        if (deadEnemy != null)
        {
            Destroy(deadEnemy.gameObject);
        }

        currentEnemy = null;

        if (rewardController != null)
        {
            rewardController.ShowRewardUI();
        }
    }

    public void EnemyAttack(BattleUnit enemy)
    {
        if (battleEnded) return;

        if (enemy == null)
        {
            Debug.LogWarning("BattleManager: No enemy to perform attack.");
            return;
        }

        StartCoroutine(EnemyAttackRoutine(enemy));
    }

    private IEnumerator EnemyAttackRoutine(BattleUnit enemy)
    {
        cont = false;
        enemyMove = enemy.GetEnemy().GetEnemyData().GetRandomMove();

        yield return new WaitUntil(() => cont);

        if (battleEnded) yield break;

        if (enemyMove == null)
        {
            Debug.LogWarning("BattleManager: Enemy selected a null move.");
            yield break;
        }

        float baseDamage = enemyMove.getDamage();

        Affinity moveAffinity = enemyMove.getType();
        Affinity playerAffinity = PlayerManager.Instance.GetAffinity();

        string effectivenessMessage;
        float multiplier = GetAffinityMultiplier(moveAffinity, playerAffinity, out effectivenessMessage);
        float finalDamage = baseDamage * multiplier;

        LastEnemyEffectivenessMessage = effectivenessMessage;

        float remainingHealth = playerManager.ApplyDamage(finalDamage);

        Debug.Log(
            "Enemy used " + enemyMove.getMoveName() +
            " | Base: " + baseDamage +
            " | Multiplier: " + multiplier +
            " | Final: " + finalDamage
        );

        if (!battleEnded && remainingHealth <= 0)
        {
            battleEnded = true;
            turnController.StopBattleFlow();
            Debug.Log("Player defeated.");
        }
    }

    public void ContinueBattle()
    {
        if (battleEnded) return;
        cont = true;
    }

    private float GetAffinityMultiplier(Affinity attack, Affinity target, out string effectivenessMessage)
    {
        effectivenessMessage = "";

        if (attack == null || target == null)
            return 1f;

        if (attack.IsStrongAgainst(target))
        {
            effectivenessMessage = "<color=red>It's super effective!</color>";
            return 2f;
        }

        if (attack.IsWeakAgainst(target))
        {
            effectivenessMessage = "<color=red>It's not very effective...</color>";
            return 0.5f;
        }

        return 1f;
    }
}