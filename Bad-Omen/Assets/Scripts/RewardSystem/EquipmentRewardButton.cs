using UnityEngine;

public class EquipmentRewardButton : ButtonScript
{
    [SerializeField] private RewardController rewardController;

    public void OnClick()
    {
        if (rewardController == null)
        {
            Debug.LogError("EquipmentRewardButton: RewardController is null.");
            return;
        }

        rewardController.ApplyRandomEquipmentUpgrade();
    }
}