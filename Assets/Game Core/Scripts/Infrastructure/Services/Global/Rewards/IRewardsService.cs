namespace GameCore.Infrastructure.Services.Global.Rewards
{
    public interface IRewardsService
    {
        void AddGold(long amount, bool autoSave = true);
    }
}