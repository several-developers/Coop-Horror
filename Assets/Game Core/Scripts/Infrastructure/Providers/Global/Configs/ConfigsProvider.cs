using GameCore.Configs.Global.Game;
using GameCore.Gameplay.InputHandlerTEMP;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Global
{
    public class ConfigsProvider : AssetsProviderBase, IConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConfigsProvider()
        {
            _gameConfig = Load<GameConfigMeta>(path: ConfigsPaths.GameConfig);
            _inputReader = Load<InputReader>(path: ConfigsPaths.InputReader); // TEMP
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameConfigMeta _gameConfig;
        private readonly InputReader _inputReader; // TEMP

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameConfigMeta GetGameConfig() => _gameConfig;
        
        // TEMP
        public InputReader GetInputReader() => _inputReader;
    }
}