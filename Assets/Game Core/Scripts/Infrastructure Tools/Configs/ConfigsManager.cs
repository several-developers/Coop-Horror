using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.InfrastructureTools.Configs
{
    public class ConfigsManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConfigsManager(ConfigScope configScope)
        {
            _configScope = configScope;
            _allConfigs = new List<ConfigMeta>();

            FindAllConfigs();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const string GameData = "Game Data/";

        private readonly ConfigScope _configScope;
        private readonly List<ConfigMeta> _allConfigs;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public T GetConfigMeta<T>() where T : ConfigMeta
        {
            Type type = typeof(T);
            return GetConfigMeta<T>(type);
        }
        
        public T GetConfigMeta<T>(Type type) where T : ConfigMeta
        {
            foreach (ConfigMeta configMeta in _allConfigs)
            {
                bool isTypeMatches = configMeta.GetType() == type;

                if (!isTypeMatches)
                    continue;

                ConfigScope configScope = configMeta.GetConfigScope();
                bool isScopeValid = configScope == _configScope;

                if (!isScopeValid)
                    continue;
                
                return configMeta as T;
            }

            LogConfigNotFound(type);
            return null;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void FindAllConfigs()
        {
            ConfigMeta[] allConfigsMeta = Resources.LoadAll<ConfigMeta>(path: GameData);

            foreach (ConfigMeta configMeta in allConfigsMeta)
                _allConfigs.Add(configMeta);
        }

        private static void LogConfigNotFound(Type configType)
        {
            string errorLog = Log.HandleLog($"Config Meta of type <gb>({configType}</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);
        }
    }
}