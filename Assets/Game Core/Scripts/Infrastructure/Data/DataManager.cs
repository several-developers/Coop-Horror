﻿using System;
using System.Collections.Generic;
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
            _allData = new List<DataBase>();

            var gamesData = new GamesData();
            var gameSettingsData = new GameSettingsData();

            _jsonUtility = new JsonUtilitySaveLoadData();

            _allData.Add(gamesData);
            _allData.Add(gameSettingsData);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const string ResetLocalDataTitle = "Reset Local Data";
        private const string LoadLocalDataTitle = "Load Local Data";
        private const string SaveLocalDataTitle = "Save Local Data";

        private readonly List<DataBase> _allData;
        private readonly ISaveLoadData _jsonUtility;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadLocalData()
        {
            int iterations = _allData.Count;

            for (int i = 0; i < iterations; i++)
            {
                DataBase data = _allData[i];
                _jsonUtility.TryLoadData(ref data);
            }
        }

        public void SaveLocalData()
        {
            int iterations = _allData.Count;

            for (int i = 0; i < iterations; i++)
            {
                DataBase data = _allData[i];
                _jsonUtility.TrySaveData(data);
            }
        }

        public void DeleteLocalData()
        {
            int iterations = _allData.Count;

            for (int i = 0; i < iterations; i++)
            {
                DataBase data = _allData[i];
                _jsonUtility.TryDeleteData(ref data);
            }
        }

        public IEnumerable<DataBase> GetAllData() => _allData;

        public DataBase GetData<T>() where T : DataBase
        {
            Type type = typeof(T);

            foreach (DataBase data in _allData)
            {
                bool isMatches = data.GetType() == type;

                if (isMatches)
                    return data;
            }

            LogDataNotFound(type);
            return null;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LogDataNotFound(Type dataType)
        {
            string errorLog = Log.HandleLog($"Data of type <gb>({dataType}</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);
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