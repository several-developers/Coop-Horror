using System;
using GameCore.Infrastructure.Data;

namespace GameCore.Infrastructure.Providers.Global.Data
{
    public class DataProvider : IDataProvider, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DataProvider()
        {
            _dataManager = new DataManager();
            _dataManager.LoadLocalData();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DataManager _dataManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dataManager.SaveLocalData();

        public DataManager GetDataManager() => _dataManager;

        public GameData GetGameData() =>
            _dataManager.GameData;

        public GameSettingsData GetGameSettingsData() =>
            _dataManager.GameSettingsData;

        public PlayerData GetPlayerData() =>
            _dataManager.PlayerData;
    }
}