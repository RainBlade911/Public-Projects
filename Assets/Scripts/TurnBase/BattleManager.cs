using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private string deathSceneName = "DeathScene";
    [SerializeField] private Animator animator;
    [SerializeField] private UnitStatsRuntime currentEnemy;
    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private NextEncounter nextEncounter;

    [SerializeField] private RewardController rewardController;
    [SerializeField] private float rewardDelay = 1.5f;
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

    private void Start()
    {
        StartBattle();
    }

    private void StartBattle()
    {
        nextEncounter.ShowEncounterScreen();

        if (rewardController != null)
            rewardController.HideRewardUI();

        battleEnded = false;
        PlayerAttackInProgress = false;
        EnemyAttackInProgress = false;
        EnemyUIActive = false;
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
        float multiplier = GetAffinityMultiplier(moveAffinity, enemyAffinity, out effectivenessMessage);
        float finalDamage = baseDamage * multiplier;

        // 2. Show player attack text immediately
        string text = "The Player used " + move.getMoveName() + "!";
        if (!string.IsNullOrEmpty(effectivenessMessage))
            text += "\n" + effectivenessMessage;

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

        particlePlayer.PlayParticleEffect(deadEnemy.transform.position);
        RemoveFromBattle(deadEnemy);

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

        rewardController?.ShowRewardUI();
    }

    private IEnumerator HandlePlayerDefeat()
    {
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

        foreach (var _ in prepared)
        {
            BattleUnit spawned = enemySpawner.SpawnNext();

            spawnedEnemyUnits.Add(spawned);

            UnitStatsRuntime stats = spawned.GetComponent<UnitStatsRuntime>();
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
