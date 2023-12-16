using GameCore.Infrastructure.Data;
using GameCore.Infrastructure.Providers.Global.Data;

namespace GameCore.Infrastructure.Services.Global.Data
{
    public class GameDataService : IGameDataService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameDataService(ISaveLoadService saveLoadService, IDataProvider dataProvider)
        {
            _saveLoadService = saveLoadService;
            _gameData = dataProvider.GetGameData();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ISaveLoadService _saveLoadService;
        private readonly GameData _gameData;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetCurrentLevel(int level, bool autoSave)
        {
            _gameData.SetCurrentLevel(level);
            SaveLocalData(autoSave);
        }

        public void IncreaseCurrentLevel(bool autoSave)
        {
            int level = _gameData.CurrentLevel;
            level++;
            SetCurrentLevel(level, autoSave);
        }

        public int GetCurrentLevel() =>
            _gameData.CurrentLevel;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SaveLocalData(bool autoSave = true)
        {
            if (!autoSave)
                return;
            
            _saveLoadService.Save();
        }
    }
}