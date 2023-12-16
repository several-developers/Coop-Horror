using GameCore.Infrastructure.Data;
using GameCore.Infrastructure.Providers.Global.Data;

namespace GameCore.Infrastructure.Services.Global.Data
{
    public class GameSettingsDataService : IGameSettingsDataService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameSettingsDataService(ISaveLoadService saveLoadService, IDataProvider dataProvider)
        {
            _saveLoadService = saveLoadService;
            _gameSettingsData = dataProvider.GetGameSettingsData();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ISaveLoadService _saveLoadService;
        private readonly GameSettingsData _gameSettingsData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetSoundVolume(float volume, bool autoSave = true)
        {
            _gameSettingsData.SetSoundVolume(volume);
            SaveLocalData(autoSave);
        }

        public void SetMusicVolume(float volume, bool autoSave = true)
        {
            _gameSettingsData.SetMusicVolume(volume);
            SaveLocalData(autoSave);
        }

        public float GetSoundVolume() =>
            _gameSettingsData.SoundVolume;

        public float GetMusicVolume() =>
            _gameSettingsData.MusicVolume;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SaveLocalData(bool autoSave = true)
        {
            if (!autoSave)
                return;

            _saveLoadService.Save();
        }
    }
}