using UnityEngine;

public class ButtonScript : MonoBehaviour
{

    BattleManager battleManager;

    private void Start()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
        Debug.Assert(battleManager != null, "BattleManager not found in the scene. Please ensure there is a BattleManager component in the scene.");
    }
    [SerializeField] public PlayerMenus playerMenus;
    public void ActionSelection(string action)
    {
        Debug.Log("Click "  + action);
        
        playerMenus.ChangeUITo(action);
    }

    public void AttackSelection(string attack)
    {
        Debug.Log("Selected attack: " + attack);

        battleManager.AttackSelected();

        //TODO: Call Attack Here
        playerMenus.ChangeUITo("Default");
    }

    public void ItemSelection(string item)
    {
        Debug.Log("Selected item: " + item);
        //TODO: Call Item Here
        playerMenus.ChangeUITo("Default");
    }

}
