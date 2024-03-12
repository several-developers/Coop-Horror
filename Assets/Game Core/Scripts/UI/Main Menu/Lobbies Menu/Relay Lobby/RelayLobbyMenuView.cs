using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.UnityServices.Auth;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using Sirenix.OdinInspector;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Zenject;

namespace GameCore.UI.MainMenu.LobbiesMenu.RelayLobby
{
    public class RelayLobbyMenuView : LobbyMenuBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(AuthenticationServiceFacade authenticationServiceFacade,
            LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby, LocalLobbyUser lobbyUser,
            ConnectionManager connectionManager)
        {
            _authenticationServiceFacade = authenticationServiceFacade;
            _lobbyServiceFacade = lobbyServiceFacade;
            _localLobby = localLobby;
            _localUser = lobbyUser;
            _connectionManager = connectionManager;
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private LobbyCreationUI _lobbyCreationUI;

        [SerializeField, Required]
        private LobbyJoiningUI _lobbyJoiningUI;

        // FIELDS: --------------------------------------------------------------------------------
        
        private const string DefaultLobbyName = "no-name";

        private AuthenticationServiceFacade _authenticationServiceFacade;
        private LobbyServiceFacade _lobbyServiceFacade;
        private LocalLobby _localLobby;
        private LocalLobbyUser _localUser;
        private ConnectionManager _connectionManager;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            _lobbyCreationUI.OnCreateClickedEvent += OnCreateClicked;

            _lobbyJoiningUI.OnJoinClickedEvent += OnJoinClicked;
            _lobbyJoiningUI.OnQuickJoinClickedEvent += OnQuickJoinClicked;
            _lobbyJoiningUI.OnRefreshLobbyEvent += OnRefreshLobby;
            _lobbyJoiningUI.OnLobbyItemClickedEvent += OnLobbyItemClicked;
        }

        protected override void Start()
        {
            base.Start();
            _lobbyJoiningUI.Show();
        }

        private void OnDestroy() =>
            _lobbyJoiningUI.Hide();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void CreateLobbyRequest(string lobbyName, bool isPrivate)
        {
            // Before sending request to lobby service, populate an empty lobby name, if necessary.
            if (string.IsNullOrEmpty(lobbyName))
                lobbyName = DefaultLobbyName;

            //BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                //UnblockUIAfterLoadingIsComplete();
                return;
            }

            var lobbyCreationAttempt = await _lobbyServiceFacade.TryCreateLobbyAsync(lobbyName,
                _connectionManager.MaxConnectedPlayers, isPrivate);

            if (lobbyCreationAttempt.Success)
            {
                _localUser.IsHost = true;
                _lobbyServiceFacade.SetRemoteLobby(lobbyCreationAttempt.Lobby);

                Debug.Log($"Created lobby with ID: {_localLobby.LobbyID} and code {_localLobby.LobbyCode}");
                _connectionManager.StartHostLobby(_localUser.DisplayName);
            }
            else
            {
                //UnblockUIAfterLoadingIsComplete();
            }
        }
        
        private async void JoinLobbyWithCodeRequest(string lobbyCode)
        {
            //BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                //UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await _lobbyServiceFacade.TryJoinLobbyAsync(null, lobbyCode);

            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                //UnblockUIAfterLoadingIsComplete();
            }
        }
        
        public async void JoinLobbyRequest(LocalLobby lobby)
        {
            //BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                //UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await _lobbyServiceFacade.TryJoinLobbyAsync(lobby.LobbyID, lobby.LobbyCode);

            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                //UnblockUIAfterLoadingIsComplete();
            }
        }
        
        private async void QueryLobbiesRequest(bool blockUI)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                return;

            if (blockUI)
            {
                //BlockUIWhileLoadingIsInProgress();
            }

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (blockUI && !playerIsAuthorized)
            {
                //UnblockUIAfterLoadingIsComplete();
                return;
            }

            await _lobbyServiceFacade.RetrieveAndPublishLobbyListAsync();

            if (blockUI)
            {
                //UnblockUIAfterLoadingIsComplete();
            }
        }
        
        private async void QuickJoinRequest()
        {
            //BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await _authenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                //UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await _lobbyServiceFacade.TryQuickJoinLobbyAsync();

            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                //UnblockUIAfterLoadingIsComplete();
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCreateClicked(string lobbyName, bool isPrivate) => CreateLobbyRequest(lobbyName, isPrivate);

        private void OnJoinClicked(string joinCode) => JoinLobbyWithCodeRequest(joinCode);

        private void OnQuickJoinClicked() => QuickJoinRequest();

        private void OnRefreshLobby(bool blockUI) => QueryLobbiesRequest(blockUI);

        private void OnLobbyItemClicked(LocalLobby data) => JoinLobbyRequest(data);

        private void OnJoinedLobby(Lobby remoteLobby)
        {
            _lobbyServiceFacade.SetRemoteLobby(remoteLobby);

            Debug.Log($"Joined lobby with code: {_localLobby.LobbyCode}, " +
                      $"Internal Relay Join Code{_localLobby.RelayJoinCode}");
            
            _connectionManager.StartClientLobby(_localUser.DisplayName);
        }
    }
}