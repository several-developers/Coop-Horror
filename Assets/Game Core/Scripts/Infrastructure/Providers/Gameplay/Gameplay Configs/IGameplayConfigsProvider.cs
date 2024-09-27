using System;
using GameCore.InfrastructureTools.Configs;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public interface IGameplayConfigsProvider
    {
        public T GetConfig<T>() where T : ConfigMeta;
        public T GetConfig<T>(Type type) where T : ConfigMeta;
    }
}