using GameCore.Configs.Game;
using GameCore.Gameplay.InputHandlerTEMP;

namespace GameCore.Infrastructure.Providers.Global
{
    public interface IConfigsProvider
    {
        GameConfigMeta GetGameConfig();
        InputReader GetInputReader(); // TEMP
    }
}