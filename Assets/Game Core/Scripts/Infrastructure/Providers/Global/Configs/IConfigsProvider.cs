using System;
using GameCore.Gameplay.Systems.InputManagement;
using GameCore.Infrastructure.Configs;

namespace GameCore.Infrastructure.Providers.Global
{
    public interface IConfigsProvider
    {
        T GetConfig<T>() where T : ConfigMeta;
        T GetConfig<T>(Type type) where T : ConfigMeta;
        InputReader GetInputReader(); // TEMP
    }
}