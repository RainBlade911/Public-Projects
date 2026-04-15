using UnityEngine;

public class MovesetRewardButton : ButtonScript
{
    [SerializeField] private RewardController rewardController;

    public void OnClick()
    {
        if (rewardController == null)
        {
            Debug.LogError("MovesetRewardButton: RewardController is null.");
            return;
        }

        rewardController.ApplyRandomMovesetUpgrade();
    }
}