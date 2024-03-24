using System;
using System.Collections;
using GameCore.Enums.Global;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public class ScenesLoaderService2 : NetworkBehaviour, IScenesLoaderService2
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnSceneStartLoadingEvent;
        public event Action OnSceneFinishedLoadingEvent;
        public event Action<bool> OnLoadingScreenStateChangedEvent;

        private bool IsNetworkSceneManagementEnabled => NetworkManager != null
                                                        && NetworkManager.SceneManager != null
                                                        && NetworkManager.NetworkConfig.EnableSceneManagement;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => DontDestroyOnLoad(gameObject);

        private void Start() =>
            SceneManager.sceneLoaded += OnSceneLoaded;

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Initializes the callback on scene events. This needs to be called right after initializing NetworkManager
        /// (after StartHost, StartClient or StartServer)
        /// </summary>
        public void AddOnSceneEventCallback()
        {
            if (IsNetworkSceneManagementEnabled)
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        /// <summary>
        /// Loads a scene asynchronously using the specified loadSceneMode, with NetworkSceneManager if on a listening
        /// server with SceneManagement enabled, or SceneManager otherwise. If a scene is loaded via SceneManager, this
        /// method also triggers the start of the loading screen.
        /// </summary>
        /// <param name="sceneName">Name or path of the Scene to load.</param>
        /// <param name="isNetwork">If true, uses NetworkSceneManager, else uses SceneManager</param>
        /// <param name="loadSceneMode">If LoadSceneMode.Single then all current Scenes will be unloaded before loading.</param>
        public void LoadScene(SceneName sceneName, bool isNetwork, LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            Action callback = null)
        {
            if (isNetwork)
            {
                if (IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress)
                {
                    if (NetworkManager.IsServer)
                    {
                        // If is active server and NetworkManager uses scene management, load scene using NetworkManager's SceneManager
                        NetworkManager.SceneManager.LoadScene(sceneName.ToString(), loadSceneMode);
                        
                        OnSceneStartLoadingEvent?.Invoke();
                    }
                }
            }
            else
            {
                // Load using SceneManager
                StartCoroutine(SceneLoaderCO(sceneName, loadSceneMode));
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ShowLoadingScreen() =>
            OnLoadingScreenStateChangedEvent?.Invoke(true);

        private void HideLoadingScreen() =>
            OnLoadingScreenStateChangedEvent?.Invoke(false);

        private static void UnloadAdditiveScenes()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                
                if (scene.isLoaded && scene != activeScene)
                    SceneManager.UnloadSceneAsync(scene);
            }
        }
        
        private IEnumerator SceneLoaderCO(SceneName sceneName, LoadSceneMode loadSceneMode, Action callback = null)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName.ToString(), loadSceneMode);

            if (loadSceneMode == LoadSceneMode.Single)
            {
                OnSceneStartLoadingEvent?.Invoke();
                
                ShowLoadingScreen();
                //m_ClientLoadingScreen.StartLoadingScreen(sceneName);
                //m_LoadingProgressManager.LocalLoadOperation = asyncLoad;
            }
            
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
                yield return null;

            callback?.Invoke();
            OnSceneFinishedLoadingEvent?.Invoke();
        }

        // RPC: -----------------------------------------------------------------------------------
        
        [ClientRpc]
        private void StopLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
        {
            HideLoadingScreen();
            //m_ClientLoadingScreen.StopLoadingScreen();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null && NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!IsSpawned || NetworkManager.ShutdownInProgress)
            {
                HideLoadingScreen();
                
                //m_ClientLoadingScreen.StopLoadingScreen();
                
                OnSceneFinishedLoadingEvent?.Invoke();
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            Debug.LogWarning("Scene Event: " + sceneEvent.SceneEventType);
            switch (sceneEvent.SceneEventType)
            {
                // Server told client to load a scene
                case SceneEventType.Load:
                    // Only executes on client
                    if (NetworkManager.IsClient)
                    {
                        // Only start a new loading screen if scene loaded in Single mode, else simply update
                        if (sceneEvent.LoadSceneMode == LoadSceneMode.Single)
                        {
                            ShowLoadingScreen();
                            //m_ClientLoadingScreen.StartLoadingScreen(sceneEvent.SceneName);
                            //m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                        else
                        {
                            //m_ClientLoadingScreen.UpdateLoadingScreen(sceneEvent.SceneName);
                            //m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                    }

                    break;

                // Server told client that all clients finished loading a scene
                case SceneEventType.LoadEventCompleted:
                    // Only executes on client
                    if (NetworkManager.IsClient)
                    {
                        HideLoadingScreen();
                        //m_ClientLoadingScreen.StopLoadingScreen();
                        //m_LoadingProgressManager.ResetLocalProgress();
                        
                        OnSceneFinishedLoadingEvent?.Invoke();
                    }

                    break;

                // Server told client to start synchronizing scenes
                case SceneEventType.Synchronize:
                {
                    // todo: this is a workaround that could be removed once MTT-3363 is done
                    // Only executes on client that is not the host
                    if (NetworkManager.IsClient && !NetworkManager.IsHost)
                    {
                        // unload all currently loaded additive scenes so that if we connect to a server with the same
                        // main scene we properly load and synchronize all appropriate scenes without loading a scene
                        // that is already loaded.
                        UnloadAdditiveScenes();
                    }

                    break;
                }

                // Client told server that they finished synchronizing
                case SceneEventType.SynchronizeComplete:
                    // Only executes on server
                    if (NetworkManager.IsServer)
                    {
                        // Send client RPC to make sure the client stops the loading screen after the server handles what it needs to after the client finished synchronizing, for example character spawning done server side should still be hidden by loading screen.
                        StopLoadingScreenClientRpc(new ClientRpcParams
                            { Send = new ClientRpcSendParams { TargetClientIds = new[] { sceneEvent.ClientId } } });
                    }
                    
                    if (NetworkManager.IsClient && !NetworkManager.IsHost)
                        OnSceneFinishedLoadingEvent?.Invoke();

                    break;
            }
        }
    }
}