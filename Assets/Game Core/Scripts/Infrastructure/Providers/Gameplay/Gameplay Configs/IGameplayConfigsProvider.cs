using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.ItemsSpawn;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Configs.Gameplay.Player;
using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;
using GameCore.Configs.Gameplay.RigPresets;
using GameCore.Configs.Gameplay.Time;
using GameCore.Configs.Gameplay.Visual;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public interface IGameplayConfigsProvider
    {
        BalanceConfigMeta GetBalanceConfig();
        PlayerConfigMeta GetPlayerConfig();
        ItemsListConfigMeta GetItemsListConfig();
        LocationsListConfigMeta GetLocationsListConfig();
        TimeConfigMeta GetTimeConfig();
        DungeonGeneratorConfigMeta GetDungeonGeneratorConfig();
        ElevatorConfigMeta GetElevatorConfig();
        PrefabsListConfigMeta GetPrefabsListConfig();
        QuestsConfigMeta GetQuestsConfig();
        QuestsItemsConfigMeta GetQuestsItemsConfig();
        DeliveryConfigMeta GetDeliveryConfig();
        RigPresetsConfigMeta GetRigPresetsConfig();
        VisualConfigMeta GetVisualConfig();
        ItemsSpawnConfigMeta GetItemsSpawnConfig();
        MonstersListConfigMeta GetMonstersListConfig();
    }
}