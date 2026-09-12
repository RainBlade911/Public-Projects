using UnityEngine;

public class BossRewardButton : ButtonScript
{
    public enum BossRewardType
    {
        EssenceOfStrength,
        EssenceOfKnowledge
    }

    [SerializeField] private BossRewardController bossRewardController;
    [SerializeField] private BossRewardType rewardType;

    public void OnClick()
    {
        if (bossRewardController == null)
        {
            Debug.LogError("BossRewardButton: BossRewardController is null.");
            return;
        }

        switch (rewardType)
        {
            case BossRewardType.EssenceOfStrength:
                bossRewardController.ApplyEssenceOfStrength();
                break;

            case BossRewardType.EssenceOfKnowledge:
                bossRewardController.ApplyEssenceOfKnowledge();
                break;
        }
    }
}