using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.UnityServices.Auth;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace GameCore.StateMachine
{
    public class SignInState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SignInState(
            IGameStateMachine gameStateMachine,
            LocalLobby localLobby,
            LocalLobbyUser localUser,
            ProfileManager profileManager,
            AuthenticationServiceFacade authenticationServiceFacade
        )
        {
            _gameStateMachine = gameStateMachine;
            _localLobby = localLobby;
            _localUser = localUser;
            _profileManager = profileManager;
            _authenticationServiceFacade = authenticationServiceFacade;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly LocalLobby _localLobby;
        private readonly LocalLobbyUser _localUser;
        private readonly ProfileManager _profileManager;
        private readonly AuthenticationServiceFacade _authenticationServiceFacade;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter() =>
            await TrySignIn();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask TrySignIn()
        {
            try
            {
                var unityAuthenticationInitOptions = new InitializationOptions();
                string profile = _profileManager.Profile;

                if (profile.Length > 0)
                    unityAuthenticationInitOptions.SetProfile(profile);

                await _authenticationServiceFacade.InitializeAndSignInAsync(unityAuthenticationInitOptions);
                OnAuthSignIn();
                //m_ProfileManager.onProfileChanged += OnProfileChanged;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                OnSignInFailed();
            }
        }

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAuthSignIn()
        {
            Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");

            _localUser.ID = AuthenticationService.Instance.PlayerId;

            // The local LobbyUser object will be hooked into UI before the LocalLobby is populated during lobby join,
            // so the LocalLobby must know about it already when that happens.
            _localLobby.AddUser(_localUser);

            EnterMainMenuState();
        }

        private void OnSignInFailed()
        {
            Debug.Log("Auth failed!");
            EnterPrepareMainMenuState();
        }
    }
}