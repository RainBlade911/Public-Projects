using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    protected BattleManager battleManager;
    [SerializeField] protected PlayerMenus playerMenus;

    protected virtual void Start()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
    }

    public virtual void ActionSelection(string action)
    {
        playerMenus.ChangeUITo(action);
    }
}
