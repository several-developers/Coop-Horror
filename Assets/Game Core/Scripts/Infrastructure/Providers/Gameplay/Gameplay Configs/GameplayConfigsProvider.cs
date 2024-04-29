﻿using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Configs.Gameplay.Player;
using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;
using GameCore.Configs.Gameplay.Time;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public class GameplayConfigsProvider : AssetsProviderBase, IGameplayConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplayConfigsProvider()
        {
            _balanceConfig = Load<BalanceConfigMeta>(path: ConfigsPaths.BalanceConfig);
            _playerConfig = Load<PlayerConfigMeta>(path: ConfigsPaths.PlayerConfig);
            _itemsListConfig = Load<ItemsListConfigMeta>(path: ConfigsPaths.ItemsListConfig);
            _locationsListConfig = Load<LocationsListConfigMeta>(path: ConfigsPaths.LocationsListConfig);
            _timeConfig = Load<TimeConfigMeta>(path: ConfigsPaths.TimeConfig);
            _dungeonGeneratorConfig = Load<DungeonGeneratorConfigMeta>(path: ConfigsPaths.DungeonGeneratorConfig);
            _elevatorConfig = Load<ElevatorConfigMeta>(path: ConfigsPaths.ElevatorConfig);
            _prefabsListConfig = Load<PrefabsListConfigMeta>(path: ConfigsPaths.PrefabsListConfig);
            _questsConfig = Load<QuestsConfigMeta>(path: ConfigsPaths.QuestsConfig);
            _questsItemsConfig = Load<QuestsItemsConfigMeta>(path: ConfigsPaths.QuestsItemsConfig);
            _deliveryConfig = Load<DeliveryConfigMeta>(path: ConfigsPaths.DeliveryConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BalanceConfigMeta _balanceConfig;
        private readonly PlayerConfigMeta _playerConfig;
        private readonly ItemsListConfigMeta _itemsListConfig;
        private readonly LocationsListConfigMeta _locationsListConfig;
        private readonly TimeConfigMeta _timeConfig;
        private readonly DungeonGeneratorConfigMeta _dungeonGeneratorConfig;
        private readonly ElevatorConfigMeta _elevatorConfig;
        private readonly PrefabsListConfigMeta _prefabsListConfig;
        private readonly QuestsConfigMeta _questsConfig;
        private readonly QuestsItemsConfigMeta _questsItemsConfig;
        private readonly DeliveryConfigMeta _deliveryConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public BalanceConfigMeta GetBalanceConfig() => _balanceConfig;
        public PlayerConfigMeta GetPlayerConfig() => _playerConfig;
        public ItemsListConfigMeta GetItemsListConfig() => _itemsListConfig;
        public LocationsListConfigMeta GetLocationsListConfig() => _locationsListConfig;
        public TimeConfigMeta GetTimeConfig() => _timeConfig;
        public DungeonGeneratorConfigMeta GetDungeonGeneratorConfig() => _dungeonGeneratorConfig;
        public ElevatorConfigMeta GetElevatorConfig() => _elevatorConfig;
        public PrefabsListConfigMeta GetPrefabsListConfig() => _prefabsListConfig;
        public QuestsConfigMeta GetQuestsConfig() => _questsConfig;
        public QuestsItemsConfigMeta GetQuestsItemsConfig() => _questsItemsConfig;
        public DeliveryConfigMeta GetDeliveryConfig() => _deliveryConfig;
    }
}