using GameCore.Infrastructure.Services.Global.Data;

namespace GameCore.Infrastructure.Services.Global.Rewards
{
    public class RewardsService : IRewardsService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RewardsService(IPlayerDataService playerDataService) =>
            _playerDataService = playerDataService;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IPlayerDataService _playerDataService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddGold(long amount, bool autoSave = true) =>
            _playerDataService.AddGold(amount, autoSave);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        public static int CalculateMineGoldReward(int baseIncome, int incomeUpgradeLevel)
        {
            int finalIncome = baseIncome + incomeUpgradeLevel;
            return finalIncome;
        }
    }
}