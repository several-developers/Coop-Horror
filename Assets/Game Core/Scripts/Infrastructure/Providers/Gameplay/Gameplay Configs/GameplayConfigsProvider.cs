using GameCore.Configs.Gameplay;
using GameCore.Configs.ItemsList;
using GameCore.Core.Configs.Player;
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
            _playerConfig = Load<PlayerConfigMeta>(path: ConfigsPaths.PlayerConfig);
            _itemsListConfig = Load<ItemsListConfigMeta>(path: ConfigsPaths.ItemsListConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameplayConfigMeta _gameplayConfig;
        private readonly PlayerConfigMeta _playerConfig;
        private readonly ItemsListConfigMeta _itemsListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameplayConfigMeta GetGameplayConfig() => _gameplayConfig;

        public PlayerConfigMeta GetPlayerConfig() => _playerConfig;

        public ItemsListConfigMeta GetItemsListConfig() => _itemsListConfig;
    }
}