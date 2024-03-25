using System;
using GameCore.Enums.Global;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public interface IScenesLoaderService
    {
        event Action OnSceneLoadEvent;
        event Action OnSceneLoadedEvent;
        event Action<bool> OnLoadingScreenStateChangedEvent; 
        void AddOnSceneEventCallback();
        void LoadScene(SceneName sceneName, bool isNetwork, LoadSceneMode loadSceneMode = LoadSceneMode.Single);
        void UnloadScene(SceneName sceneName, bool isNetwork);
    }
}