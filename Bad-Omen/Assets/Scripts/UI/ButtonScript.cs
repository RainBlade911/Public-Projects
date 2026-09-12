using TMPro;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    protected BattleManager battleManager;
    [SerializeField] protected PlayerMenus playerMenus;
    [SerializeField] PlayerActionUI playerActionUI;
    [SerializeField] TurnController turnController;

    protected virtual void Start()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
    }

    public virtual void ActionSelection(string action)
    {
        playerMenus.ChangeUITo(action);
    }

    public void SetActionMessage(string text)
    {
        playerActionUI.SetActionText(text);
    }

    public void ActionSelected()
    {
        turnController.ActionSelected();
    }

    public void GoHome()
    {
        playerMenus.ChangeUITo("Default");
    }
}
