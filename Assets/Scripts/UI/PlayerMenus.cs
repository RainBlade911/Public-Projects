using TMPro;
using UnityEngine;

public class PlayerMenus : MonoBehaviour
{

    [SerializeField] private GameObject attackMenu;
    [SerializeField] private GameObject ItemMenu;
    [SerializeField] private GameObject BlockMenu;
    [SerializeField] private GameObject DefaultMenu;

    [SerializeField] private AttackButton[] attackButtons;

    public void FillAttackMenu()
    {
        Move[] moves = PlayerManager.Instance.moves;
        int count = Mathf.Min(moves.Length, attackButtons.Length);
        for (int i = 0; i < count; i++)
        {
            
            if (moves[i] != null)
            {
                attackButtons[i].Initialize(i, moves[i].getMoveName());
            }
            else
            {
                attackButtons[i].Initialize(i, "Empty");
            }
        }
        for(int i = count; i < attackButtons.Length; i++)
        {
            attackButtons[i].Initialize(i, "Empty");
        }
    }
    private void Start()
    {
        SetSoloActive(DefaultMenu);
    }

    public void ChangeUITo(string menu)
    {
        Debug.Log("Changing menu to: " + menu);
        switch (menu)
        {
            case "Attack":
                Debug.Log("Attack");
                SetSoloActive(attackMenu);
                FillAttackMenu();
                break;
            case "Block":
                Debug.Log("Block");
                SetSoloActive(BlockMenu);
                break;
            case "Use Item":
                Debug.Log("Use Item");
                SetSoloActive(ItemMenu);
                break;
            case "Default":
                Debug.Log("Default");
                SetSoloActive(DefaultMenu);
                break;
            default:
                Debug.Log("Unknown menu: " + menu);
                Debug.Log("Default");
                SetSoloActive(DefaultMenu);
                break;
        }


    }

    private void SetSoloActive(GameObject activeMenu)
    {
        Debug.Log("Setting active menu: " + activeMenu.name);
        SetAllInactive();
        activeMenu.SetActive(true);
    }

    public void SetAllInactive()
    {
        attackMenu.SetActive(false);
        ItemMenu.SetActive(false);
        BlockMenu.SetActive(false);
        DefaultMenu.SetActive(false);
    }

    
}
