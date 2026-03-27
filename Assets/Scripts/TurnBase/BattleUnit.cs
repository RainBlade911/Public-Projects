using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private string unitName = "Unit";
    [SerializeField] private bool isPlayer;
    [SerializeField] private Health health;
    [SerializeField] private CombatStats stats;

    public string UnitName => unitName;
    public bool IsPlayer => isPlayer;
    public Health Health => health;
    public CombatStats Stats => stats;

    private void Reset()
    {
        health = GetComponent<Health>();
        stats = GetComponent<CombatStats>();
    }
}