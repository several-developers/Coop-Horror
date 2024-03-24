using System;
using GameCore.Enums.Global;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public interface IScenesLoaderService2
    {
        event Action OnSceneStartLoadingEvent;
        event Action OnSceneFinishedLoadingEvent;
        event Action<bool> OnLoadingScreenStateChangedEvent; 
        void AddOnSceneEventCallback();
        void LoadScene(SceneName sceneName, bool isNetwork, LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            Action callback = null);
    }
}