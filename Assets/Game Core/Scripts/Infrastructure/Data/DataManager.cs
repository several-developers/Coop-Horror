using System;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Data
{
    [Serializable]
    public class DataManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DataManager()
        {
            _gamesData = new GamesData();
            _gameSettingsData = new GameSettingsData();

            _jsonUtility = new JsonUtilitySaveLoadData();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(Constants.Settings)]
        [BoxGroup(EditorConstants.GamesData, showLabel: false), SerializeField]
        private GamesData _gamesData;

        [BoxGroup(EditorConstants.GameSettings, showLabel: false), SerializeField]
        private GameSettingsData _gameSettingsData;

        // PROPERTIES: ----------------------------------------------------------------------------

        public GamesData GamesData => _gamesData ??= new();
        public GameSettingsData GameSettingsData => _gameSettingsData ??= new();

        // FIELDS: --------------------------------------------------------------------------------

        private const string ResetLocalDataTitle = "Reset Local Data";
        private const string LoadLocalDataTitle = "Load Local Data";
        private const string SaveLocalDataTitle = "Save Local Data";

        private readonly ISaveLoadData _jsonUtility;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadLocalData()
        {
            _jsonUtility.TryLoadData(ref _gamesData);
            _jsonUtility.TryLoadData(ref _gameSettingsData);
        }

        public void SaveLocalData()
        {
            _jsonUtility.TrySaveData(_gamesData);
            _jsonUtility.TrySaveData(_gameSettingsData);
        }
        
        public void DeleteLocalData()
        {
            _jsonUtility.TryDeleteData(ref _gamesData);
            _jsonUtility.TryDeleteData(ref _gameSettingsData);
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(ButtonHeight = 35), GUIColor(0.4f, 1f, 0.4f)]
        [LabelText(SaveLocalDataTitle)]
        private void DebugSaveLocalData() => SaveLocalData();

        [Button(ButtonHeight = 35), GUIColor(0.5f, 0.5f, 1)]
        [LabelText(LoadLocalDataTitle)]
        private void DebugLoadLocalData() => LoadLocalData();

        [Button(ButtonHeight = 25), GUIColor(1f, 0.4f, 0.4f)]
        [LabelText(ResetLocalDataTitle)]
        private void DebugResetLocalData()
        {
            DeleteLocalData();
            SaveLocalData();
            LoadLocalData();
        }
    }
}