using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.ItemsSpawn;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Configs.Gameplay.MonstersGenerator;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Configs.Gameplay.Player;
using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;
using GameCore.Configs.Gameplay.RigPresets;
using GameCore.Configs.Gameplay.Time;
using GameCore.Configs.Gameplay.Train;
using GameCore.Configs.Gameplay.Visual;
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
            _trainConfig = Load<TrainConfigMeta>(path: ConfigsPaths.TrainConfig);
            _elevatorConfig = Load<ElevatorConfigMeta>(path: ConfigsPaths.ElevatorConfig);
            _prefabsListConfig = Load<PrefabsListConfigMeta>(path: ConfigsPaths.PrefabsListConfig);
            _questsConfig = Load<QuestsConfigMeta>(path: ConfigsPaths.QuestsConfig);
            _questsItemsConfig = Load<QuestsItemsConfigMeta>(path: ConfigsPaths.QuestsItemsConfig);
            _rigPresetsConfig = Load<RigPresetsConfigMeta>(path: ConfigsPaths.RigPresetsConfig);
            _visualConfig = Load<VisualConfigMeta>(path: ConfigsPaths.VisualConfig);
            _itemsSpawnConfig = Load<ItemsSpawnConfigMeta>(path: ConfigsPaths.ItemsSpawnConfig);
            _monstersListConfig = Load<MonstersListConfigMeta>(path: ConfigsPaths.MonstersListConfig);
            _monstersGeneratorConfig = Load<MonstersGeneratorConfigMeta>(path: ConfigsPaths.MonstersGeneratorConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BalanceConfigMeta _balanceConfig;
        private readonly PlayerConfigMeta _playerConfig;
        private readonly ItemsListConfigMeta _itemsListConfig;
        private readonly LocationsListConfigMeta _locationsListConfig;
        private readonly TimeConfigMeta _timeConfig;
        private readonly TrainConfigMeta _trainConfig;
        private readonly ElevatorConfigMeta _elevatorConfig;
        private readonly PrefabsListConfigMeta _prefabsListConfig;
        private readonly QuestsConfigMeta _questsConfig;
        private readonly QuestsItemsConfigMeta _questsItemsConfig;
        private readonly RigPresetsConfigMeta _rigPresetsConfig;
        private readonly VisualConfigMeta _visualConfig;
        private readonly ItemsSpawnConfigMeta _itemsSpawnConfig;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly MonstersGeneratorConfigMeta _monstersGeneratorConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public BalanceConfigMeta GetBalanceConfig() => _balanceConfig;
        public PlayerConfigMeta GetPlayerConfig() => _playerConfig;
        public ItemsListConfigMeta GetItemsListConfig() => _itemsListConfig;
        public LocationsListConfigMeta GetLocationsListConfig() => _locationsListConfig;
        public TimeConfigMeta GetTimeConfig() => _timeConfig;
        public TrainConfigMeta GetTrainConfig() => _trainConfig;
        public ElevatorConfigMeta GetElevatorConfig() => _elevatorConfig;
        public PrefabsListConfigMeta GetPrefabsListConfig() => _prefabsListConfig;
        public QuestsConfigMeta GetQuestsConfig() => _questsConfig;
        public QuestsItemsConfigMeta GetQuestsItemsConfig() => _questsItemsConfig;
        public RigPresetsConfigMeta GetRigPresetsConfig() => _rigPresetsConfig;
        public VisualConfigMeta GetVisualConfig() => _visualConfig;
        public ItemsSpawnConfigMeta GetItemsSpawnConfig() => _itemsSpawnConfig;
        public MonstersListConfigMeta GetMonstersListConfig() => _monstersListConfig;
        public MonstersGeneratorConfigMeta GetMonstersGeneratorConfig() => _monstersGeneratorConfig;
    }
}