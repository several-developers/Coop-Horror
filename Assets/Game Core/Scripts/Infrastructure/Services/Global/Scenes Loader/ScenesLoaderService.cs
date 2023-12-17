using System;
using System.Collections;
using GameCore.Utilities;
using GameCore.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public class ScenesLoaderService : IScenesLoaderService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ScenesLoaderService(ICoroutineRunner coroutineRunner) =>
            _coroutineRunner = coroutineRunner;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnSceneStartLoading;
        public event Action OnSceneFinishedLoading;
        
        private readonly ICoroutineRunner _coroutineRunner;

        private bool _isSceneLoading;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadScene(SceneName sceneName, Action callback = null)
        {
            if (_isSceneLoading)
                return;
            
            _coroutineRunner.StartCoroutine(SceneLoader(sceneName, callback));
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private IEnumerator SceneLoader(SceneName sceneName, Action callback = null)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName.ToString());

            _isSceneLoading = true;
            OnSceneStartLoading?.Invoke();

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
                yield return null;

            _isSceneLoading = false;
            
            callback?.Invoke();
            OnSceneFinishedLoading?.Invoke();
        }
    }
}