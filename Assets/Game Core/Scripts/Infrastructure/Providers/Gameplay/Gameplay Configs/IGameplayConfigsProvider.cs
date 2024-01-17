using GameCore.Configs.Gameplay;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Core.Configs.Player;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public interface IGameplayConfigsProvider
    {
        GameplayConfigMeta GetGameplayConfig();
        PlayerConfigMeta GetPlayerConfig();
        ItemsListConfigMeta GetItemsListConfig();
        LocationsListConfigMeta GetLocationsListConfig();
    }
}