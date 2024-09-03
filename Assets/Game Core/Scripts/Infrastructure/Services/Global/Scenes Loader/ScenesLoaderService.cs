using System;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Global;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.Services.Global
{
    public class ScenesLoaderService : NetworkBehaviour, IScenesLoaderService
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        private bool IsNetworkSceneManagementEnabled => NetworkManager != null
                                                        && NetworkManager.SceneManager != null
                                                        && NetworkManager.NetworkConfig.EnableSceneManagement;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnSceneLoadEvent = delegate { };
        public event Action OnSceneLoadedEvent = delegate { };
        public event Action OnSceneUnloadedEvent = delegate { };
        public event Action<bool> OnLoadingScreenStateChangedEvent = delegate { };

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => DontDestroyOnLoad(gameObject);

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
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
        public void LoadScene(SceneName sceneName, bool isNetwork, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (isNetwork)
            {
                bool canLoadScene = IsSpawned &&
                                    IsNetworkSceneManagementEnabled &&
                                    !NetworkManager.ShutdownInProgress &&
                                    NetworkManager.IsServer;

                if (!canLoadScene)
                    return;

                // If is active server and NetworkManager uses scene management,
                // load scene using NetworkManager's SceneManager
                NetworkManager.SceneManager.LoadScene(sceneName.ToString(), loadSceneMode);

                OnSceneLoadEvent.Invoke();
            }
            else
            {
                // Load using SceneManager
                SceneLoader(sceneName, loadSceneMode);
            }
        }

        public void UnloadScene(SceneName sceneName, bool isNetwork)
        {
            if (isNetwork)
            {
                Scene scene = SceneManager.GetSceneByName(sceneName.ToString());
                NetworkSceneManager networkSceneManager = NetworkManager.Singleton.SceneManager;
                networkSceneManager.UnloadScene(scene);
            }
            else
            {
                SceneManager.UnloadSceneAsync(sceneName.ToString());
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ShowLoadingScreen() =>
            OnLoadingScreenStateChangedEvent?.Invoke(true);

        private void HideLoadingScreen() =>
            OnLoadingScreenStateChangedEvent?.Invoke(false);

        private void UnloadAdditiveScenes()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                bool unload = scene.isLoaded && scene != activeScene;

                if (!unload)
                    continue;

                SceneManager.UnloadSceneAsync(scene);
            }
        }

        private async UniTaskVoid SceneLoader(SceneName sceneName, LoadSceneMode loadSceneMode, Action callback = null)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            // AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName.ToString(), loadSceneMode);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName.ToString(), loadSceneMode);

            if (loadSceneMode == LoadSceneMode.Single)
            {
                OnSceneLoadEvent.Invoke();

                ShowLoadingScreen();
                //m_ClientLoadingScreen.StartLoadingScreen(sceneName);
                //m_LoadingProgressManager.LocalLoadOperation = asyncLoad;
            }

            // Wait until the asynchronous scene fully loads
            while (!asyncOperation.isDone)
            {
#warning ЗАДЕРЖКА ПОД ВОПРОСОМ
                await UniTask.Delay(millisecondsDelay: 100);
            }

            callback?.Invoke();
            OnSceneLoadedEvent.Invoke();
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

                OnSceneLoadedEvent.Invoke();
            }
        }

        private void OnSceneUnloaded(Scene scene) =>
            OnSceneUnloadedEvent.Invoke();

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                // Server told client to load a scene
                case SceneEventType.Load:
                    Load();
                    break;

                // Server told client that all clients finished loading a scene
                case SceneEventType.LoadEventCompleted:
                    LoadEventCompleted();
                    break;

                // Server told client to start synchronizing scenes
                case SceneEventType.Synchronize:
                    Synchronize();
                    break;

                // Client told server that they finished synchronizing
                case SceneEventType.SynchronizeComplete:
                    SynchronizeComplete();
                    break;
            }

            // LOCAL METHODS: -----------------------------

            void Load()
            {
                // Only executes on client
                if (!NetworkManager.IsClient)
                    return;

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

            void LoadEventCompleted()
            {
                // Only executes on client
                if (!NetworkManager.IsClient)
                    return;

                HideLoadingScreen();
                //m_ClientLoadingScreen.StopLoadingScreen();
                //m_LoadingProgressManager.ResetLocalProgress();

                OnSceneLoadedEvent.Invoke();
            }

            void Synchronize()
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
            }

            void SynchronizeComplete()
            {
                // Only executes on server
                if (NetworkManager.IsServer)
                {
                    // Send client RPC to make sure the client stops the loading screen after the server handles
                    // what it needs to after the client finished synchronizing, for example character spawning
                    // done server side should still be hidden by loading screen.

                    var clientRpcParams = new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams { TargetClientIds = new[] { sceneEvent.ClientId } }
                    };

                    StopLoadingScreenClientRpc(clientRpcParams);
                }

                if (NetworkManager.IsClient && !NetworkManager.IsHost)
                    OnSceneLoadedEvent.Invoke();
            }
        }
    }
}