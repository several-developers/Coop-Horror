using GameCore.Configs.Gameplay;
using GameCore.Core.Configs.Player;

namespace GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs
{
    public interface IGameplayConfigsProvider
    {
        GameplayConfigMeta GetGameplayConfig();
        PlayerConfigMeta GetPlayerConfig();
    }
}