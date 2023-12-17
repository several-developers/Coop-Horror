using GameCore.Configs.Game;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Global
{
    public class ConfigsProvider : AssetsProviderBase, IConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConfigsProvider()
        {
            _gameConfig = Load<GameConfigMeta>(path: ConfigsPaths.GameConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameConfigMeta _gameConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameConfigMeta GetGameConfig() => _gameConfig;
    }
}