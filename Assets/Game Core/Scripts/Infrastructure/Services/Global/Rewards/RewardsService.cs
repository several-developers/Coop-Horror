using GameCore.Infrastructure.Services.Global.Data;

namespace GameCore.Infrastructure.Services.Global.Rewards
{
    public class RewardsService : IRewardsService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RewardsService()
        {
        }

        // FIELDS: --------------------------------------------------------------------------------


        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddGold(long amount, bool autoSave = true)
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        public static int CalculateMineGoldReward(int baseIncome, int incomeUpgradeLevel)
        {
            int finalIncome = baseIncome + incomeUpgradeLevel;
            return finalIncome;
        }
    }
}