using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    private string unitName;
    [SerializeField] private bool isPlayer;
    [SerializeField] private UnitStatsRuntime enemy;
    private float speed;
  

    private void Reset()
    {
    }

    private void Start()
    {
        SetSpeed();
        SetName();
    }

    private void SetSpeed()
    {
        if (isPlayer)
        {
            speed = PlayerManager.Instance.getCurrentSpeed();
        }
        else
        {
            speed = enemy.GetEnemyData().GetSpeed();
        }
    }

    private void SetName()
    {
        if (isPlayer)
        {
            unitName = "Player";
        }
        else
        {
            unitName = enemy.GetEnemyData().GetEnemyName();
        }
    }

    public string GetName()
    {
        return unitName;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public bool IsPlayer()
    {
        return isPlayer;
    }

    public UnitStatsRuntime GetEnemy()
    {
        if(!isPlayer)
        {
            return enemy;
        }
        else
        {
            return null;
        }
    }

    public Sprite GetSprite()
    {
        if (isPlayer)
        {
            return PlayerManager.Instance.GetSprite();
        }
        else
        {
            return enemy.GetEnemyData().GetSprite();
        }
    }
}