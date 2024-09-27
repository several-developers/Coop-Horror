using System;
using GameCore.Gameplay.Systems.InputManagement;
using GameCore.InfrastructureTools.Configs;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Global
{
    public sealed class ConfigsProvider : AssetsProviderBase, IConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConfigsProvider()
        {
            _configsManager = new ConfigsManager(ConfigScope.Global);
            _inputReader = Load<InputReader>(path: ConfigsPaths.InputReader); // TEMP
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ConfigsManager _configsManager;
        private readonly InputReader _inputReader; // TEMP

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public T GetConfig<T>() where T : ConfigMeta =>
            _configsManager.GetConfigMeta<T>();
        
        public T GetConfig<T>(Type type) where T : ConfigMeta =>
            _configsManager.GetConfigMeta<T>(type);
        
        // TEMP
        public InputReader GetInputReader() => _inputReader;
    }
}