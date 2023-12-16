namespace GameCore.Infrastructure.Services.Global.Data
{
    public interface IPlayerDataService
    {
        void AddGold(long gold, bool autoSave = true);
        void SetGold(long gold, bool autoSave = true);
        void RemoveGold(long gold, bool autoSave = true);
        long GetGold();
    }
}