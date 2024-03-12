using GameCore.Utilities;

namespace GameCore.Gameplay.Network.UnityServices.Lobbies
{
    /// <summary>
    /// Keep updated on changes to a joined lobby, at a speed compliant with Lobby's rate limiting.
    /// </summary>
    public class JoinedLobbyContentHeartbeat
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JoinedLobbyContentHeartbeat(IUpdateRunner updateRunner, LocalLobby localLobby, LocalLobbyUser localUser,
            LobbyServiceFacade lobbyServiceFacade)
        {
            _updateRunner = updateRunner;
            _localLobby = localLobby;
            _localUser = localUser;
            _lobbyServiceFacade = lobbyServiceFacade;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IUpdateRunner _updateRunner;
        private readonly LocalLobby _localLobby;
        private readonly LocalLobbyUser _localUser;
        private readonly LobbyServiceFacade _lobbyServiceFacade;

        private int _awaitingQueryCount;
        private bool _shouldPushData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void BeginTracking()
        {
            _updateRunner.Subscribe(OnUpdate, updatePeriod: 1.5f);

            _localLobby.OnChangedEvent += OnLocalLobbyChanged;

            // Ensure the initial presence of a new player is pushed to the lobby; otherwise, when a non-host joins,
            // the LocalLobby never receives their data until they push something new.
            _shouldPushData = true;
        }

        public void EndTracking()
        {
            _shouldPushData = false;
            _updateRunner.Unsubscribe(OnUpdate);

            _localLobby.OnChangedEvent -= OnLocalLobbyChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnLocalLobbyChanged(LocalLobby lobby)
        {
            // When the player leaves, their LocalLobby is cleared out but maintained.
            if (string.IsNullOrEmpty(lobby.LobbyID))
                EndTracking();

            _shouldPushData = true;
        }

        /// <summary>
        /// If there have been any data changes since the last update, push them to Lobby.
        /// (Unless we're already awaiting a query, in which case continue waiting.)
        /// </summary>
        private async void OnUpdate(float dt)
        {
            if (_awaitingQueryCount > 0)
                return;


            if (_localUser.IsHost)
                _lobbyServiceFacade.DoLobbyHeartbeat(dt);

            if (!_shouldPushData)
                return;

            _shouldPushData = false;

            if (_localUser.IsHost)
            {
                // todo this should disappear once we use await correctly.
                // todo This causes issues at the moment if OnSuccess isn't called properly

                _awaitingQueryCount++;
                await _lobbyServiceFacade.UpdateLobbyDataAsync(_localLobby.GetDataForUnityServices());
                _awaitingQueryCount--;
            }

            _awaitingQueryCount++;
            await _lobbyServiceFacade.UpdatePlayerDataAsync(_localUser.GetDataForUnityServices());
            _awaitingQueryCount--;
        }
    }
}