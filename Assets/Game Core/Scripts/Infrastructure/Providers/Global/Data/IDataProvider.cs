using GameCore.Infrastructure.Data;

namespace GameCore.Infrastructure.Providers.Global.Data
{
    public interface IDataProvider
    {
        DataManager GetDataManager();
        GameData GetGameData();
        GameSettingsData GetGameSettingsData();
        PlayerData GetPlayerData();
    }
}