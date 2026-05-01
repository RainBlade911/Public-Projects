using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public Move enemyMove;
    public string LastEnemyEffectivenessMessage { get; private set; } = "";

    List<BattleUnit> spawnedEnemyUnits = new List<BattleUnit>();
    List<UnitStatsRuntime> enemyStatsRuntimes = new List<UnitStatsRuntime>();

    private BattleUnit selectedEnemy;

    private bool inputLocked = false;
    public bool playerAttackInProgress = false;

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
        LastEnemyEffectivenessMessage = "";
    }

    public bool AttackSelected(Move move)
    {
        if (battleEnded || inputLocked || move == null) return false;

        selectedEnemy = selectionManager.GetSelectedEnemy();
        if (selectedEnemy == null) return false;

        currentEnemy = selectedEnemy.GetEnemy();
        if (currentEnemy == null) return false;

        inputLocked = true;
        playerAttackInProgress = true;

        StartCoroutine(AttackRoutine(move));
        return true;
    }

    private IEnumerator AttackRoutine(Move move)
    {
        animator.SetTrigger("AttackTrigger");

        Vector3 hitPos = currentEnemy.transform.position + move.getOffset();

       if (move.getMoveEffectPrefab() != null)
        {
            particlePlayer.PlayAttackParticle(
                hitPos,
                move.getMoveEffectPrefab(),
                currentEnemy.transform
            );
        }

        // small delay not for particle
        yield return new WaitForSeconds(0.2f);

        float baseDamage = move.getDamage();

        Affinity moveAffinity = move.getType();
        Affinity enemyAffinity = currentEnemy.GetEnemyData().GetAffinity();

        string effectivenessMessage;
        float multiplier = GetAffinityMultiplier(moveAffinity, enemyAffinity, out effectivenessMessage);
        float finalDamage = baseDamage * multiplier;

        float remainingHealth = currentEnemy.ApplyDamage(finalDamage);
        float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());
        Debug.Log(
    $"[PLAYER ATTACK] Target: {currentEnemy.name} | Base: {baseDamage} | Multiplier: {multiplier} | Final: {finalDamage}");

        PlayerManager.Instance.SetAffinity(moveAffinity);

     
        if (playerActionUI != null)
        {
            string text = "The Player used " + move.getMoveName() + "!";
            if (!string.IsNullOrEmpty(effectivenessMessage))
                text += "\n" + effectivenessMessage;

            playerActionUI.SetActionText(text);
            playerActionUI.SetTextActive();
        }

        inputLocked = false;
        playerAttackInProgress = false;
    }

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
        turnController.StopBattleFlow();
        yield return new WaitForSeconds(rewardDelay);

        if (rewardController != null)
            rewardController.ShowRewardUI();
    }

    public void EnemyAttack(BattleUnit enemy)
    {
        if (battleEnded) return;
        StartCoroutine(EnemyAttackRoutine(enemy));
    }

    private IEnumerator EnemyAttackRoutine(BattleUnit enemy)
    {
        cont = false;
        enemyMove = enemy.GetEnemy().GetEnemyData().GetRandomMove();

        float baseDamage = enemyMove.getDamage();

        Affinity moveAffinity = enemyMove.getType();
        Affinity playerAffinity = playerManager.GetAffinity();

        string effectivenessMessage = ""; // reset every attack
        float multiplier = GetAffinityMultiplier(moveAffinity, playerAffinity, out effectivenessMessage);
        float finalDamage = baseDamage * multiplier;

        LastEnemyEffectivenessMessage = effectivenessMessage; // store current, not previous

        // NOW show the UI with the correct message
        enemyBattleUIHandler.ShowEnemyAttackMessage(enemy.GetName(), enemyMove, effectivenessMessage);

        EnemyAnimatorHandler enemyAnim = enemy.GetComponent<EnemyAnimatorHandler>();
        if (enemyAnim != null)
            enemyAnim.PlayAttack();

        yield return new WaitUntil(() => cont);

        float remainingHealth = playerManager.ApplyDamage(finalDamage);

        if (!battleEnded && remainingHealth <= 0)
        {
            battleEnded = true;
            turnController.StopBattleFlow();
            StartCoroutine(HandlePlayerDefeat());
        }
    }


    private IEnumerator HandlePlayerDefeat()
    {
        yield return new WaitForSeconds(1f);
        SceneTransitionManager.LoadSceneWithTransition(deathSceneName);
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