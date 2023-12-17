using GameCore.Configs.Game;

namespace GameCore.Infrastructure.Providers.Global
{
    public interface IConfigsProvider
    {
        GameConfigMeta GetGameConfig();
    }
}