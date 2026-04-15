using TMPro;
using UnityEngine;

public class EnemyBattleUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject enemyAttackUI;
    [SerializeField] private BattleManager battleManager;

    private TextMeshProUGUI attackText;
    public bool Continue { get; private set; }

    private void Awake()
    {
        attackText = enemyAttackUI.GetComponentInChildren<TextMeshProUGUI>();
        enemyAttackUI.SetActive(false);
    }

    public void ShowEnemyAttackMessage(string enemyName, Move attack)
    {
        Continue = false;

        enemyAttackUI.SetActive(true);
        attackText.text = $"{enemyName} is attacking with {attack.getMoveName()}!\nIt deals {attack.getDamage()} damage!";
    }

    public void OnContinue()
    {
        Continue = true;
        battleManager.ContinueBattle();
        enemyAttackUI.SetActive(false);
    }
}
