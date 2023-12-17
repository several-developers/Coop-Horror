using System;
using GameCore.Enums;

namespace GameCore.Infrastructure.Services.Global
{
    public interface IScenesLoaderService
    {
        event Action OnSceneStartLoading;
        event Action OnSceneFinishedLoading;
        void LoadScene(SceneName sceneName, Action callback = null);
        void LoadSceneNetwork(SceneName sceneName);
    }
}