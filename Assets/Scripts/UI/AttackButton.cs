using TMPro;
using UnityEngine;

public class AttackButton : ButtonScript
{
    [SerializeField] private TextMeshProUGUI attackLabel;

    private int moveIndex;

    public void Initialize(int index, string moveName)
    {
        moveIndex = index;
        attackLabel.text = moveName;
    }

    public void OnClick()
    {
        Move selectedMove = battleManager.getMove(moveIndex);

        if (selectedMove == null)
        {
            Debug.LogWarning("AttackButton: No move assigned at index " + moveIndex);
            playerMenus.ChangeUITo("Default");
            return;
        }

        float currentMana = PlayerManager.Instance.GetCurrentMana();
        float moveCost = selectedMove.getManaCost();

        if (currentMana < moveCost)
        {
            Debug.Log(
                "Not enough mana to use " +
                selectedMove.getMoveName() +
                ". Current Mana: " + currentMana +
                ", Required: " + moveCost
            );

            playerMenus.ChangeUITo("Default");
            return;
        }

       
        SetAttackMessage();

       
        if (battleManager.AttackSelected(selectedMove))
        {
            ActionSelected();
        }
        else
        {
            Debug.Log("Attack failed — no enemy selected.");
            
            playerMenus.ChangeUITo("Default");
        }
    }


    public void SetAttackMessage()
    {
        SetActionMessage("Player attacks with " + attackLabel.text + "!");
    }
}