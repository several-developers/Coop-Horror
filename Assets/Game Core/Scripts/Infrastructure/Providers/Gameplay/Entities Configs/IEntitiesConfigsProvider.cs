using GameCore.Infrastructure.Configs.Gameplay.Entities;

namespace GameCore.Infrastructure.Providers.Gameplay.EntitiesConfigs
{
    public interface IEntitiesConfigsProvider
    {
        TEntityConfigType GetConfig<TEntityConfigType>() where TEntityConfigType : EntityConfigMeta;
    }
}