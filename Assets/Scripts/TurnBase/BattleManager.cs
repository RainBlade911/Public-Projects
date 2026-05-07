using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private string deathSceneName = "DeathScene";
    [SerializeField] private Animator animator;
    [SerializeField] private UnitStatsRuntime currentEnemy;
    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private NextEncounter nextEncounter;

    [Header("Normal Rewards")]
    [SerializeField] private RewardController rewardController;
    [SerializeField] private float rewardDelay = 1.5f;

    [Header("Boss Rewards")]
    [SerializeField] private BossRewardController bossRewardController;

    [SerializeField] private PlayerActionUI playerActionUI;

    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyBattleUIHandler enemyBattleUIHandler;
    [SerializeField] private RaycastSelectionManager selectionManager;
    [SerializeField] private ParticlePlayer particlePlayer;

    private bool cont = false;

    private bool battleEnded = false;
    public bool BattleEnded => battleEnded;

    public bool PlayerAttackInProgress { get; private set; } = false;
    public bool EnemyAttackInProgress { get; private set; } = false;
    public bool EnemyUIActive { get; private set; } = false;

    public Move enemyMove;
    public string LastEnemyEffectivenessMessage { get; private set; } = "";

    private readonly List<BattleUnit> spawnedEnemyUnits = new();
    private readonly List<UnitStatsRuntime> enemyStatsRuntimes = new();

    private BattleUnit selectedEnemy;
    private bool inputLocked = false;

    // Added for new reward behavior
    private int defeatedEnemyCount = 0;
    private bool currentBattleIsBoss = false;

    private void Start()
    {
        StartBattle();
    }

    private void StartBattle()
    {
        nextEncounter.ShowEncounterScreen();

        if (rewardController != null)
            rewardController.HideRewardUI();

        // Added for boss reward UI
        if (bossRewardController != null)
            bossRewardController.HideBossRewardUI();

        battleEnded = false;
        PlayerAttackInProgress = false;
        EnemyAttackInProgress = false;
        EnemyUIActive = false;

        // Added for tracking reward count and boss battles
        defeatedEnemyCount = 0;
        currentBattleIsBoss = false;
    }

    private void SetBattleEnded()
    {
        if (battleEnded) return;

        battleEnded = true;
        turnController.OnBattleEnded();
    }

    // ============================
    // PLAYER ATTACK
    // ============================

    public bool AttackSelected(Move move)
    {
        if (battleEnded || inputLocked || move == null) return false;

        selectedEnemy = selectionManager.GetSelectedEnemy();
        if (selectedEnemy == null) return false;

        currentEnemy = selectedEnemy.GetEnemy();
        if (currentEnemy == null) return false;

        inputLocked = true;
        PlayerAttackInProgress = true;

        StartCoroutine(AttackRoutine(move));
        return true;
    }

    private IEnumerator AttackRoutine(Move move)
    {
        // 1. Player attack animation FIRST
        animator.SetTrigger("AttackTrigger");

        Vector3 hitPos = currentEnemy.transform.position + move.getOffset();
        ParticleSystem hitEffect = move.getMoveEffectPrefab();

        float baseDamage = move.getDamage();
        Affinity moveAffinity = move.getType();
        Affinity enemyAffinity = currentEnemy.GetEnemyData().GetAffinity();

        string effectivenessMessage;
        float affinityMultiplier = GetAffinityMultiplier(moveAffinity, enemyAffinity, out effectivenessMessage);

        // Added for boss reward buffs
        // Strike uses EOS only.
        // Non-Strike moves use EOK only.
        // EOS and EOK should never stack together on the same attack.
        float bossRewardMultiplier = playerManager.GetDamageMultiplierForMove(move);

        float finalDamage = baseDamage * affinityMultiplier * bossRewardMultiplier;

        // 2. Show player attack text immediately
        string text = "The Player used " + move.getMoveName() + "!";
        if (!string.IsNullOrEmpty(effectivenessMessage))
            text += "\n" + effectivenessMessage;

        // Added debug-style player text so you can verify the correct multiplier is working
        //text += "\nDamage Multiplier: x" + bossRewardMultiplier.ToString("0.00");

        playerActionUI.SetActionText(text);
        playerActionUI.SetTextActive();

        // 3. Play particle WHILE the attack animation is happening
        if (hitEffect != null)
        {
            particlePlayer.PlayAttackParticle(hitPos, hitEffect, currentEnemy.transform);
            yield return new WaitForSeconds(hitEffect.main.duration);
        }

        // 4. Play enemy damage reaction BEFORE applying damage
        var enemyAnim = currentEnemy.GetComponent<EnemyAnimatorHandler>();
        if (enemyAnim != null)
        {
            enemyAnim.ResetDamageTrigger();
            enemyAnim.PlayDamage();
        }

        // 5. Apply damage
        currentEnemy.ApplyDamage(finalDamage);
        playerManager.ApplyManaCost(move.getManaCost());
        playerManager.SetAffinity(moveAffinity);

        Debug.Log(
            "BattleManager: Final player damage calculated. " +
            "Base: " + baseDamage +
            ", Affinity Multiplier: " + affinityMultiplier +
            ", Boss Reward Multiplier: " + bossRewardMultiplier +
            ", Final Damage: " + finalDamage
        );

        // 6. Hide UI
        playerActionUI.HideText();

        inputLocked = false;
        PlayerAttackInProgress = false;
    }

    // ============================
    // ENEMY ATTACK
    // ============================

    public void EnemyAttack(BattleUnit enemy)
    {
        if (battleEnded) return;
        StartCoroutine(EnemyAttackRoutine(enemy));
    }

    private IEnumerator EnemyAttackRoutine(BattleUnit enemy)
    {
        cont = false;

        // THINKING TIME
        EnemyUIActive = true;
        playerActionUI.SetActionText(". . .");
        playerActionUI.SetTextActive();

        yield return new WaitForSeconds(2f);

        playerActionUI.HideText();

        // NOW begin real attack
        EnemyAttackInProgress = true;

        enemyMove = enemy.GetEnemy().GetEnemyData().GetRandomMove();

        if (enemyMove == null)
        {
            Debug.LogWarning("BattleManager: Enemy tried to attack but had no move.");
            EnemyAttackInProgress = false;
            EnemyUIActive = false;
            yield break;
        }

        float baseDamage = enemyMove.getDamage();
        Affinity moveAffinity = enemyMove.getType();
        Affinity playerAffinity = playerManager.GetAffinity();

        string effectivenessMessage;
        float multiplier = GetAffinityMultiplier(moveAffinity, playerAffinity, out effectivenessMessage);
        float finalDamage = baseDamage * multiplier;

        LastEnemyEffectivenessMessage = effectivenessMessage;

        var anim = enemy.GetComponent<EnemyAnimatorHandler>();
        if (anim != null)
        {
            anim.ResetAttackTrigger();
            anim.PlayAttack();
        }

        enemyBattleUIHandler.ShowEnemyAttackMessage(enemy.GetName(), enemyMove, effectivenessMessage);

        // Wait for Continue
        yield return new WaitUntil(() => cont || battleEnded);

        EnemyUIActive = false;

        if (battleEnded)
        {
            EnemyAttackInProgress = false;
            yield break;
        }

        float remainingHealth = playerManager.ApplyDamage(finalDamage);

        if (remainingHealth <= 0)
        {
            SetBattleEnded();
            StartCoroutine(HandlePlayerDefeat());
        }

        EnemyAttackInProgress = false;
    }

    public void ContinueBattle()
    {
        if (battleEnded) return;
        cont = true;
    }

    // ============================
    // ENEMY DEATH
    // ============================

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        if (battleEnded) return;

        // Added so normal rewards can trigger once per defeated enemy
        defeatedEnemyCount++;

        particlePlayer.PlayParticleEffect(deadEnemy.transform.position);
        RemoveFromBattle(deadEnemy);
        Debug.Log("Dead Enemy Score: " + deadEnemy.GetEnemyData().GetKillScore());
        Scorekeeper.Instance.addScore(deadEnemy.GetEnemyData().GetKillScore());

        if (enemyStatsRuntimes.Count > 0)
        {
            if (enemyStatsRuntimes.Count == 1)
                HandleOneEnemyLeft(spawnedEnemyUnits[0]);

            return;
        }

        StartCoroutine(HandleEnemyDefeatRoutine());
    }

    private IEnumerator HandleEnemyDefeatRoutine()
    {
        SetBattleEnded();

        yield return new WaitForSeconds(rewardDelay);

        // Added boss reward behavior
        if (currentBattleIsBoss)
        {
            if (bossRewardController != null)
                bossRewardController.ShowBossRewardUI();
            else
                Debug.LogError("BattleManager: BossRewardController is not assigned.");

            yield break;
        }

        // Added multiple normal reward behavior
        if (rewardController != null)
        {
            rewardController.SetPendingRewardChoices(defeatedEnemyCount);
            rewardController.ShowRewardUI();
        }
        else
        {
            Debug.LogError("BattleManager: RewardController is not assigned.");
        }
    }

    private IEnumerator HandlePlayerDefeat()
    {
        Scorekeeper.Instance.CalculateFinalScore();
        yield return new WaitForSeconds(1f);
        SceneTransitionManager.LoadSceneWithTransition(deathSceneName);
    }

    // ============================
    // HELPERS
    // ============================

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

    private void RemoveFromBattle(UnitStatsRuntime unit)
    {
        BattleUnit battleUnit = unit.GetComponent<BattleUnit>();

        int index = enemyStatsRuntimes.IndexOf(unit);
        if (index >= 0)
        {
            enemyStatsRuntimes.RemoveAt(index);
            spawnedEnemyUnits.RemoveAt(index);
        }

        EnemyManager.Instance.UnregisterEnemy(battleUnit);
        turnController.RemoveBattleUnit(battleUnit);
        Destroy(unit.gameObject);
    }

    public void HandleOneEnemyLeft(BattleUnit enemy)
    {
        currentEnemy = enemy.GetEnemy();
        selectedEnemy = enemy;
        selectionManager.KeepSelected(enemy);
    }

    public void SpawnEnemies()
    {
        int index = nextEncounter.SelectedEncounterIndex;
        var prepared = enemySpawner.GetPreparedEnemies(index);

        // Added to know whether to show normal rewards or boss rewards after victory
        currentBattleIsBoss = enemySpawner.IsPreparedEncounterBoss(index);

        foreach (var _ in prepared)
        {
            BattleUnit spawned = enemySpawner.SpawnNext();

            if (spawned == null)
            {
                Debug.LogError("BattleManager: EnemySpawner returned null while spawning.");
                continue;
            }

            spawnedEnemyUnits.Add(spawned);

            UnitStatsRuntime stats = spawned.GetComponent<UnitStatsRuntime>();

            if (stats == null)
            {
                Debug.LogError("BattleManager: Spawned enemy is missing UnitStatsRuntime.");
                continue;
            }

            enemyStatsRuntimes.Add(stats);

            stats.OnDied += HandleEnemyDied;
        }
    }

    public void BeginBattleAfterEncounter()
    {
        int index = nextEncounter.SelectedEncounterIndex;
        enemySpawner.BeginSpawningEncounter(index);
        SpawnEnemies();
        turnController.OnBattleStart();
    }

    public Move getMove(int index)
    {
        return PlayerManager.Instance.GetMove(index);
    }
}