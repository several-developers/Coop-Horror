using System;
using System.Collections;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Utilities;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Lifecycle
{
    public class ApplicationController : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby,
            ISubscriber<QuitApplicationMessage> quitApplicationSub)
        {
            _lobbyServiceFacade = lobbyServiceFacade;
            _localLobby = localLobby;
            _quitApplicationSub = quitApplicationSub;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private LobbyServiceFacade _lobbyServiceFacade;
        private LocalLobby _localLobby;
        private ISubscriber<QuitApplicationMessage> _quitApplicationSub;
        private IDisposable _subscriptions;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            IDisposable disposable = _quitApplicationSub.Subscribe(QuitGame);
            DisposableGroup group = new();
            group.Add(disposable);
            _subscriptions = group;

            Application.wantsToQuit += OnWantToQuit;
            Application.targetFrameRate = 120;
        }

        private void OnDestroy()
        {
            _subscriptions?.Dispose();
            _lobbyServiceFacade?.EndTracking();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        /// <summary>
        /// In builds, if we are in a lobby and try to send a Leave request on application quit,
        /// it won't go through if we're quitting on the same frame.
        /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
        /// </summary>
        private IEnumerator LeaveBeforeQuit()
        {
            // We want to quit anyways, so if anything happens while trying to leave the Lobby,
            // log the exception then carry on.
            try
            {
                _lobbyServiceFacade.EndTracking();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            
            yield return null;
            
            Application.Quit();
        }
        
        private static void QuitGame(QuitApplicationMessage message)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID);
            
            if (!canQuit)
                StartCoroutine(routine: LeaveBeforeQuit());
            
            return canQuit;
        }
    }
}