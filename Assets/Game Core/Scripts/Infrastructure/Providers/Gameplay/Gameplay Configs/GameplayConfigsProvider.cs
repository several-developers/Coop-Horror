using System;
using GameCore.InfrastructureTools.Configs;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public sealed class GameplayConfigsProvider : IGameplayConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplayConfigsProvider() =>
            _configsManager = new ConfigsManager(ConfigScope.GameplayScene);

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ConfigsManager _configsManager;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public T GetConfig<T>() where T : ConfigMeta =>
            _configsManager.GetConfigMeta<T>();

        public T GetConfig<T>(Type type) where T : ConfigMeta =>
            _configsManager.GetConfigMeta<T>(type);
    }
}