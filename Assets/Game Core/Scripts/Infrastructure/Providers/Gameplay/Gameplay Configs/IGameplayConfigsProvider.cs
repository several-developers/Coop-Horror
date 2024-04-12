using GameCore.Configs.Gameplay;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Configs.Gameplay.Time;
using GameCore.Configs.Gameplay.Player;
using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public interface IGameplayConfigsProvider
    {
        GameplayConfigMeta GetGameplayConfig();
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
    }
}