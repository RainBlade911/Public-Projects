using UnityEngine;

public class PlayerMenus : MonoBehaviour
{

    [SerializeField] private GameObject attackMenu;
    [SerializeField] private GameObject ItemMenu;
    [SerializeField] private GameObject BlockMenu;
    [SerializeField] private GameObject DefaultMenu;
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
        attackMenu.SetActive(false);
        ItemMenu.SetActive(false);
        BlockMenu.SetActive(false);
        DefaultMenu.SetActive(false);
        activeMenu.SetActive(true);
    }
}
