using GameCore.Configs.Gameplay;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public class GameplayConfigsProvider : AssetsProviderBase, IGameplayConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplayConfigsProvider()
        {
            _gameplayConfig = Load<GameplayConfigMeta>(path: ConfigsPaths.GameplayConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameplayConfigMeta _gameplayConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public GameplayConfigMeta GetGameplayConfig() => _gameplayConfig;
    }
}