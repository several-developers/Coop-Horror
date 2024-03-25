using System;
using GameCore.Enums.Global;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public interface IScenesLoaderServiceDeprecated
    {
        event Action<LoadSceneMode> OnSceneStartLoading;
        event Action<LoadSceneMode> OnSceneFinishedLoading;
        void LoadScene(SceneName sceneName, Action callback = null);
        void LoadSceneAdditive(SceneName sceneName, Action callback = null);
        void LoadSceneNetwork(SceneName sceneName, Action callback = null);
        void LoadSceneNetworkAdditive(SceneName sceneName, Action callback = null);
        void UnloadScene(SceneName sceneName, Action callback = null);
        void UnloadSceneNetwork(SceneName sceneName, Action callback = null);
    }
}