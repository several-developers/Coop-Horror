using GameCore.Configs.Gameplay.Entities;

namespace GameCore.Infrastructure.Providers.Gameplay.EntitiesConfigs
{
    public interface IEntitiesConfigsProvider
    {
        OutdoorChestConfigMeta GetOutdoorChestConfig();
    }
}