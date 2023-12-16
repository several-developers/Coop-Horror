using GameCore.Configs.Gameplay;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public interface IGameplayConfigsProvider
    {
        GameplayConfigMeta GetGameplayConfig();
    }
}