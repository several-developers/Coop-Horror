namespace GameCore.Infrastructure.Services.Global.Data
{
    public interface IGameDataService
    {
        void SetCurrentLevel(int level, bool autoSave = true);
        void IncreaseCurrentLevel(bool autoSave = true);
        int GetCurrentLevel();
    }
}