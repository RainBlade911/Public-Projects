using UnityEngine;

public class PlayerMenus : MonoBehaviour
{
    [SerializeField] private GameObject attackMenu;
    [SerializeField] private GameObject ItemMenu;
    [SerializeField] private GameObject BlockMenu;
    [SerializeField] private GameObject DefaultMenu;

    [SerializeField] private AttackButton[] attackButtons;
    [SerializeField] private SupportItemButton[] itemButtons;

    public void FillAttackMenu()
    {
        Move[] playerMoves = PlayerManager.Instance.moves.ToArray();
        int count = Mathf.Min(playerMoves.Length, attackButtons.Length);

        for (int i = 0; i < count; i++)
        {
            if (playerMoves[i] != null)
            {
                attackButtons[i].Initialize(i, playerMoves[i].getMoveName());
            }
            else
            {
                attackButtons[i].Initialize(i, "Empty");
            }
        }

        for (int i = count; i < attackButtons.Length; i++)
        {
            attackButtons[i].Initialize(i, "Empty");
        }
    }

    public void FillItemMenu()
    {
        string[] playerItems = PlayerManager.Instance.GetItems().ToArray();
        int count = Mathf.Min(playerItems.Length, itemButtons.Length);

        for (int i = 0; i < count; i++)
        {
            itemButtons[i].Initialize(i, playerItems[i]);
        }

        for (int i = count; i < itemButtons.Length; i++)
        {
            itemButtons[i].Initialize(i, "Empty");
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
                FillItemMenu();
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