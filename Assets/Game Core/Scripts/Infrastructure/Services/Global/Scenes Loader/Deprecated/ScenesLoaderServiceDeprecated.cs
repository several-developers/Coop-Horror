using System;
using System.Collections;
using System.Collections.Generic;
using GameCore.Enums.Global;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public class ScenesLoaderServiceDeprecated : IScenesLoaderServiceDeprecated
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ScenesLoaderServiceDeprecated(ICoroutineRunner coroutineRunner) =>
            _coroutineRunner = coroutineRunner;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<LoadSceneMode> OnSceneStartLoading;
        public event Action<LoadSceneMode> OnSceneFinishedLoading;

        private readonly ICoroutineRunner _coroutineRunner;

        private bool _isSceneLoading;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadScene(SceneName sceneName, Action callback = null)
        {
            if (_isSceneLoading)
                return;

            _coroutineRunner.StartCoroutine(SceneLoader(sceneName, LoadSceneMode.Single, callback));
        }

        public void LoadSceneAdditive(SceneName sceneName, Action callback = null)
        {
            if (_isSceneLoading)
                return;

            _coroutineRunner.StartCoroutine(SceneLoader(sceneName, LoadSceneMode.Additive, callback));
        }

        public void LoadSceneNetwork(SceneName sceneName, Action callback = null) =>
            LoadSceneNetwork(sceneName, LoadSceneMode.Single, callback);

        public void LoadSceneNetworkAdditive(SceneName sceneName, Action callback = null) =>
            LoadSceneNetwork(sceneName, LoadSceneMode.Additive, callback);

        public void UnloadScene(SceneName sceneName, Action callback = null) =>
            SceneManager.UnloadSceneAsync(sceneName.ToString());

        public void UnloadSceneNetwork(SceneName sceneName, Action callback = null)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName.ToString());
            NetworkSceneManager networkSceneManager = NetworkManager.Singleton.SceneManager;
            networkSceneManager.UnloadScene(scene);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadSceneNetwork(SceneName sceneName, LoadSceneMode loadSceneMode, Action callback = null)
        {
            if (_isSceneLoading)
                return;

            _isSceneLoading = true;

            NetworkSceneManager networkSceneManager = NetworkManager.Singleton.SceneManager;
            networkSceneManager.LoadScene(sceneName.ToString(), loadSceneMode);

            networkSceneManager.OnLoadEventCompleted += OnSceneLoaded;

            // LOCAL METHODS: -----------------------------

            void OnSceneLoaded(string localSceneName, LoadSceneMode sceneMode, List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                _isSceneLoading = false;
                networkSceneManager.OnLoadEventCompleted -= OnSceneLoaded;
                callback?.Invoke();
            }
        }

        private IEnumerator SceneLoader(SceneName sceneName, LoadSceneMode loadSceneMode, Action callback = null)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName.ToString(), loadSceneMode);

            _isSceneLoading = true;
            OnSceneStartLoading?.Invoke(loadSceneMode);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
                yield return null;

            _isSceneLoading = false;

            callback?.Invoke();
            OnSceneFinishedLoading?.Invoke(loadSceneMode);
        }
    }
}