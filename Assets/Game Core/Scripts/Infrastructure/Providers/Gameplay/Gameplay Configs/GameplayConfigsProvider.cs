using GameCore.Configs.Gameplay;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Configs.Gameplay.Time;
using GameCore.Configs.Gameplay.Player;
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
            _locationsListConfig = Load<LocationsListConfigMeta>(path: ConfigsPaths.LocationsListConfig);
            _timeConfig = Load<TimeConfigMeta>(path: ConfigsPaths.TimeConfig);
            _dungeonGeneratorConfig = Load<DungeonGeneratorConfigMeta>(path: ConfigsPaths.DungeonGeneratorConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameplayConfigMeta _gameplayConfig;
        private readonly PlayerConfigMeta _playerConfig;
        private readonly ItemsListConfigMeta _itemsListConfig;
        private readonly LocationsListConfigMeta _locationsListConfig;
        private readonly TimeConfigMeta _timeConfig;
        private readonly DungeonGeneratorConfigMeta _dungeonGeneratorConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameplayConfigMeta GetGameplayConfig() => _gameplayConfig;
        public PlayerConfigMeta GetPlayerConfig() => _playerConfig;
        public ItemsListConfigMeta GetItemsListConfig() => _itemsListConfig;
        public LocationsListConfigMeta GetLocationsListConfig() => _locationsListConfig;
        public TimeConfigMeta GetTimeConfig() => _timeConfig;
        public DungeonGeneratorConfigMeta GetDungeonGeneratorConfig() => _dungeonGeneratorConfig;
    }
}