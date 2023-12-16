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
            _gameData = new GameData();
            _gameSettingsData = new GameSettingsData();
            _playerData = new PlayerData();

            _jsonUtility = new JsonUtilitySaveLoadData();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(Constants.Settings)]
        [BoxGroup(EditorConstants.GameData, showLabel: false), SerializeField]
        private GameData _gameData;

        [BoxGroup(EditorConstants.GameSettings, showLabel: false), SerializeField]
        private GameSettingsData _gameSettingsData;

        [BoxGroup(EditorConstants.PlayerData, showLabel: false), SerializeField]
        private PlayerData _playerData;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public GameData GameData => _gameData ??= new();
        public GameSettingsData GameSettingsData => _gameSettingsData ??= new();
        public PlayerData PlayerData => _playerData ??= new();

        // FIELDS: --------------------------------------------------------------------------------

        private const string ResetLocalDataTitle = "Reset Local Data";
        private const string LoadLocalDataTitle = "Load Local Data";
        private const string SaveLocalDataTitle = "Save Local Data";

        private readonly ISaveLoadData _jsonUtility;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadLocalData()
        {
            _jsonUtility.TryLoadData(ref _gameData);
            _jsonUtility.TryLoadData(ref _gameSettingsData);
            _jsonUtility.TryLoadData(ref _playerData);
        }

        public void SaveLocalData()
        {
            _jsonUtility.TrySaveData(_gameData);
            _jsonUtility.TrySaveData(_gameSettingsData);
            _jsonUtility.TrySaveData(_playerData);
        }
        
        public void DeleteLocalData()
        {
            _jsonUtility.TryDeleteData(ref _gameData);
            _jsonUtility.TryDeleteData(ref _gameSettingsData);
            _jsonUtility.TryDeleteData(ref _playerData);
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