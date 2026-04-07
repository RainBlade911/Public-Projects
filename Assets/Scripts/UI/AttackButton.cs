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
        battleManager.AttackSelected(selectedMove);

        //ActionSelection("Default");
    }

}
