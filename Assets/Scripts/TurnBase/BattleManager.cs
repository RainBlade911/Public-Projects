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

    // Rewards System
    [SerializeField] private RewardController rewardController;
    [SerializeField] private float rewardDelay = 1.5f;

    // Enemy Spawning + Selection
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyBattleUIHandler enemyBattleUIHandler;
    [SerializeField] private RaycastSelectionManager selectionManager;
    [SerializeField] private ParticlePlayer particlePlayer;

    private bool waitingForRewardChoice = false;

    private bool cont = false;
    private bool battleEnded = false;

    public Move enemyMove;

    // Multi-enemy support
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


        // Reset reward UI
        if (rewardController != null)
        {
            rewardController.HideRewardUI();
        }

        battleEnded = false;

        
    }

    private void OnDestroy()
    {
        if (currentEnemy != null)
        {
            currentEnemy.OnDied -= HandleEnemyDied;
        }
    }

    public bool AttackSelected(Move move)
    {
        if (battleEnded) return false;
        if (inputLocked) return false;   // prevent UI spam
        if (move == null) return false;

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
            ParticleSystem effect = particlePlayer.PlayAttackParticle(hitPos, move.getMoveEffectPrefab(), currentEnemy.transform);

            yield return new WaitForSeconds(effect.main.duration);
        }


        float remainingHealth = currentEnemy.ApplyDamage(move.getDamage());
        float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());

        inputLocked = false;
        playerAttackInProgress = false;
    }


    //public bool AttackSelected(Move move)
    //{
    //    if (battleEnded) return false;

    //    if (move == null)
    //    {
    //        Debug.LogWarning("BattleManager: Selected move was null.");
    //        return false;
    //    }

    //    // Get selected enemy
    //    selectedEnemy = selectionManager.GetSelectedEnemy();

    //    if (selectedEnemy == null)
    //    {
    //        Debug.LogWarning("BattleManager: No enemy selected.");
    //        return false; 
    //    }

    //    currentEnemy = selectedEnemy.GetEnemy();

    //    if (currentEnemy == null)
    //    {
    //        Debug.LogWarning("BattleManager: No enemy to attack.");
    //        return false;
    //    }

    //    // Perform attack
    //    float remainingHealth = currentEnemy.ApplyDamage(move.getDamage());
    //    float remainingMana = PlayerManager.Instance.ApplyManaCost(move.getManaCost());

    //    //do the attack animation
    //    animator.SetTrigger("AttackTrigger");

    //    particlePlayer.PlayAttackParticle(currentEnemy.transform.position, move.getMoveEffectPrefab());

    //    Debug.Log("SelectedEnemy: " + selectedEnemy);
    //    Debug.Log("currentEnemy: " + currentEnemy);

    //    return true; 
    //}

    private void HandleEnemyDied(UnitStatsRuntime deadEnemy)
    {
        if (battleEnded) return;

        // Remove from turn order + internal lists
        particlePlayer.PlayParticleEffect(deadEnemy.transform.position);
        RemoveFromBattle(deadEnemy);
       

        if (enemyStatsRuntimes.Count > 0)
        {
            if (enemyStatsRuntimes.Count == 1)
            {
                BattleUnit lastEnemy = spawnedEnemyUnits[0];
                HandleOneEnemyLeft(lastEnemy);
            }

            return;
        }

        StartCoroutine(HandleEnemyDefeatRoutine());

    }


    private IEnumerator HandleEnemyDefeatRoutine()
    {
        Debug.Log("All enemies defeated.");

        turnController.StopBattleFlow();

        yield return new WaitForSeconds(rewardDelay);

        if (rewardController != null)
            rewardController.ShowRewardUI();
    }

    public Move getMove(int i)
    {
        return PlayerManager.Instance.GetMove(i);
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

        // Show attack message on screen-space UI
        enemyBattleUIHandler.ShowEnemyAttackMessage(enemy.GetName(), enemyMove);

        EnemyAnimatorHandler enemyAnim = enemy.GetComponent<EnemyAnimatorHandler>();
        if (enemyAnim != null)
        {
            enemyAnim.PlayAttack();
        }

        yield return new WaitUntil(() => cont);

        if (battleEnded) yield break;

        float remainingHealth = playerManager.ApplyDamage(enemyMove.getDamage());

        if (!battleEnded && remainingHealth <= 0)
        {
            battleEnded = true;
            turnController.StopBattleFlow();
            StartCoroutine(HandlePlayerDefeat());
        }
    }

    private IEnumerator HandlePlayerDefeat()
    {
        Debug.Log("Player defeated.");

        yield return new WaitForSeconds(1f);

        SceneTransitionManager.LoadSceneWithTransition(deathSceneName);
    }


    public void ContinueBattle()
    {
        if (battleEnded) return;
        cont = true;
    }

    private void RemoveFromBattle(UnitStatsRuntime unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("BattleManager: Attempted to remove null unit from battle.");
            return;
        }

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

    public List<BattleUnit> GetSpawnedEnemies()
    {
        return spawnedEnemyUnits;
    }

    public void SpawnEnemies()
    {
        int index = nextEncounter.SelectedEncounterIndex;
        List<BattleUnit> prepared = enemySpawner.GetPreparedEnemies(index);

        foreach (var _ in prepared)
        {
            BattleUnit spawned = enemySpawner.SpawnNext();

            spawnedEnemyUnits.Add(spawned);

            UnitStatsRuntime stats = spawned.GetComponent<UnitStatsRuntime>();
            enemyStatsRuntimes.Add(stats);

            stats.OnDied += HandleEnemyDied;

            EnemyWorldUIHandler worldUI = spawned.GetComponentInChildren<EnemyWorldUIHandler>();
            if (worldUI != null)
                worldUI.Bind(stats);
        }
    }

    public void BeginBattleAfterEncounter()
    {
        int index = nextEncounter.SelectedEncounterIndex;

        enemySpawner.BeginSpawningEncounter(index);
        SpawnEnemies();

        turnController.OnBattleStart();
    }

}
