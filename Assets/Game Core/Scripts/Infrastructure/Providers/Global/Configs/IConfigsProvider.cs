using GameCore.Configs.Global.Game;
using GameCore.Gameplay.InputManagement;

namespace GameCore.Infrastructure.Providers.Global
{
    public interface IConfigsProvider
    {
        GameConfigMeta GetGameConfig();
        InputReader GetInputReader(); // TEMP
    }
}