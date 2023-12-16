using GameCore.Infrastructure.Data;
using GameCore.Infrastructure.Providers.Global.Data;

namespace GameCore.Infrastructure.Services.Global.Data
{
    public class PlayerDataService : IPlayerDataService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerDataService(ISaveLoadService saveLoadService, IDataProvider dataProvider)
        {
            _saveLoadService = saveLoadService;
            _playerData = dataProvider.GetPlayerData();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ISaveLoadService _saveLoadService;
        private readonly PlayerData _playerData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddGold(long gold, bool autoSave = true)
        {
            long newGold = _playerData.Gold + gold;
            
            _playerData.SetGold(newGold);
            SaveLocalData(autoSave);
        }

        public void SetGold(long gold, bool autoSave = true)
        {
            _playerData.SetGold(gold);
            SaveLocalData(autoSave);
        }

        public void RemoveGold(long gold, bool autoSave = true)
        {
            _playerData.RemoveGold(gold);
            SaveLocalData(autoSave);
        }

        public long GetGold() =>
            _playerData.Gold;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SaveLocalData(bool autoSave = true)
        {
            if (!autoSave)
                return;
            
            _saveLoadService.Save();
        }
    }
}