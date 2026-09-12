using UnityEngine;

public class SupportRewardButton : ButtonScript
{
    [SerializeField] private RewardController rewardController;

    public void OnClick()
    {
        if (rewardController == null)
        {
            Debug.LogError("SupportRewardButton: RewardController is null.");
            return;
        }

        rewardController.ApplyRandomSupportItemUpgrade();
    }
}