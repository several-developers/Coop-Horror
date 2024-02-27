using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCore.Gameplay.Network.UnityServices.Other;
using GameCore.Utilities;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.UnityServices.Lobbies
{
    /// <summary>
    /// An abstraction layer between the direct calls into the Lobby API and the outcomes you actually want.
    /// </summary>
    public class LobbyServiceFacade : IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LobbyServiceFacade(IUpdateRunner updateRunner, LocalLobby localLobby, LocalLobbyUser localUser)
        {
            _updateRunner = updateRunner;
            _localLobby = localLobby;
            _localUser = localUser;

            _lobbyApiInterface = new LobbyAPIInterface(); // TEMP
            
            // TEMP
            _joinedLobbyContentHeartbeat = new JoinedLobbyContentHeartbeat(updateRunner, localLobby, localUser,
                lobbyServiceFacade: this);
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Lobby CurrentUnityLobby { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        // The heartbeat must be rate-limited to 5 calls per 30 seconds.
        // We'll aim for longer in case periods don't align.
        private const float HeartbeatPeriod = 8;

        private readonly IUpdateRunner _updateRunner;
        private readonly LocalLobby _localLobby;
        private readonly LocalLobbyUser _localUser;
        private readonly LobbyAPIInterface _lobbyApiInterface;
        private readonly JoinedLobbyContentHeartbeat _joinedLobbyContentHeartbeat;

        private RateLimitCooldown _rateLimitQuery;
        private RateLimitCooldown _rateLimitJoin;
        private RateLimitCooldown _rateLimitQuickJoin;
        private RateLimitCooldown _rateLimitHost;

        private float _heartbeatTime;
        private bool m_IsTracking;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            //See https://docs.unity.com/lobby/rate-limits.html
            _rateLimitQuery = new RateLimitCooldown(1f);
            _rateLimitJoin = new RateLimitCooldown(3f);
            _rateLimitQuickJoin = new RateLimitCooldown(10f);
            _rateLimitHost = new RateLimitCooldown(3f);
        }

        public void Dispose() => EndTracking();

        public void SetRemoteLobby(Lobby lobby)
        {
            CurrentUnityLobby = lobby;
            _localLobby.ApplyRemoteData(lobby);
        }

        public void BeginTracking()
        {
            if (m_IsTracking)
                return;

            m_IsTracking = true;

            // 2s update cadence is arbitrary and is here to demonstrate the fact that this update can be rather
            // infrequent the actual rate limits are tracked via the RateLimitCooldown objects defined above.
            _updateRunner.Subscribe(UpdateLobby, updatePeriod: 2f);
            _joinedLobbyContentHeartbeat.BeginTracking();
        }

        /// <summary>
        /// Lobby requires a periodic ping to detect rooms that are still active, in order to mitigate "zombie" lobbies.
        /// </summary>
        public void DoLobbyHeartbeat(float dt)
        {
            _heartbeatTime += dt;

            if (_heartbeatTime <= HeartbeatPeriod)
                return;

            _heartbeatTime -= HeartbeatPeriod;
            try
            {
                _lobbyApiInterface.SendHeartbeatPing(CurrentUnityLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                // If Lobby is not found and if we are not the host, it has already been deleted.
                // No need to publish the error here.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !_localUser.IsHost)
                    PublishError(e);
            }
        }

        public Task EndTracking()
        {
            var task = Task.CompletedTask;

            if (CurrentUnityLobby != null)
            {
                CurrentUnityLobby = null;

                string lobbyId = _localLobby?.LobbyID;

                if (!string.IsNullOrEmpty(lobbyId))
                {
                    if (_localUser.IsHost)
                        task = DeleteLobbyAsync(lobbyId);
                    else
                        task = LeaveLobbyAsync(lobbyId);
                }

                _localUser.ResetState();
                _localLobby?.Reset(_localUser);
            }

            if (!m_IsTracking)
                return task;

            _updateRunner.Unsubscribe(UpdateLobby);
            m_IsTracking = false;
            _heartbeatTime = 0;
            _joinedLobbyContentHeartbeat.EndTracking();

            return task;
        }

        /// <summary>
        /// Attempt to create a new lobby and then join it.
        /// </summary>
        public async Task<(bool Success, Lobby Lobby)> TryCreateLobbyAsync(string lobbyName, int maxPlayers,
            bool isPrivate)
        {
            if (!_rateLimitHost.CanCall)
            {
                Debug.LogWarning("Create Lobby hit the rate limit.");
                return (false, null);
            }

            try
            {
                var lobby = await _lobbyApiInterface.CreateLobby(AuthenticationService.Instance.PlayerId, lobbyName,
                    maxPlayers, isPrivate, _localUser.GetDataForUnityServices(), null);

                return (true, lobby);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                    _rateLimitHost.PutOnCooldown();
                else
                    PublishError(e);
            }

            return (false, null);
        }

        /// <summary>
        /// Attempt to join an existing lobby. Will try to join via code, if code is null - will try to join via ID.
        /// </summary>
        public async Task<(bool Success, Lobby Lobby)> TryJoinLobbyAsync(string lobbyId, string lobbyCode)
        {
            if (!_rateLimitJoin.CanCall ||
                (lobbyId == null && lobbyCode == null))
            {
                Debug.LogWarning("Join Lobby hit the rate limit.");
                return (false, null);
            }

            try
            {
                if (!string.IsNullOrEmpty(lobbyCode))
                {
                    var lobby = await _lobbyApiInterface.JoinLobbyByCode(AuthenticationService.Instance.PlayerId,
                        lobbyCode, _localUser.GetDataForUnityServices());

                    return (true, lobby);
                }
                else
                {
                    var lobby = await _lobbyApiInterface.JoinLobbyById(AuthenticationService.Instance.PlayerId,
                        lobbyId, _localUser.GetDataForUnityServices());

                    return (true, lobby);
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                    _rateLimitJoin.PutOnCooldown();
                else
                    PublishError(e);
            }

            return (false, null);
        }

        /// <summary>
        /// Attempt to join the first lobby among the available lobbies that match the filtered onlineMode.
        /// </summary>
        public async Task<(bool Success, Lobby Lobby)> TryQuickJoinLobbyAsync()
        {
            if (!_rateLimitQuickJoin.CanCall)
            {
                Debug.LogWarning("Quick Join Lobby hit the rate limit.");
                return (false, null);
            }

            try
            {
                var lobby = await _lobbyApiInterface.QuickJoinLobby(AuthenticationService.Instance.PlayerId,
                    _localUser.GetDataForUnityServices());

                return (true, lobby);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                    _rateLimitQuickJoin.PutOnCooldown();
                else
                    PublishError(e);
            }

            return (false, null);
        }

        /// <summary>
        /// Used for getting the list of all active lobbies, without needing full info for each.
        /// </summary>
        public async Task RetrieveAndPublishLobbyListAsync()
        {
            if (!_rateLimitQuery.CanCall)
            {
                Debug.LogWarning("Retrieve Lobby list hit the rate limit. Will try again soon...");
                return;
            }

            try
            {
                var response = await _lobbyApiInterface.QueryAllLobbies();
                //m_LobbyListFetchedPub.Publish(new LobbyListFetchedMessage(LocalLobby.CreateLocalLobbies(response)));
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                    _rateLimitQuery.PutOnCooldown();
                else
                    PublishError(e);
            }
        }

        public async Task<Lobby> ReconnectToLobbyAsync(string lobbyId)
        {
            try
            {
                return await _lobbyApiInterface.ReconnectToLobby(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                // If Lobby is not found and if we are not the host, it has already been deleted.
                // No need to publish the error here.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !_localUser.IsHost)
                    PublishError(e);
            }

            return null;
        }

        /// <summary>
        /// Attempt to leave a lobby
        /// </summary>
        public async Task LeaveLobbyAsync(string lobbyId)
        {
            string uasId = AuthenticationService.Instance.PlayerId;
            try
            {
                await _lobbyApiInterface.RemovePlayerFromLobby(uasId, lobbyId);
            }
            catch (LobbyServiceException e)
            {
                // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                if (e.Reason != LobbyExceptionReason.LobbyNotFound && !_localUser.IsHost)
                {
                    PublishError(e);
                }
            }
        }

        public async void RemovePlayerFromLobbyAsync(string uasId, string lobbyId)
        {
            if (_localUser.IsHost)
            {
                try
                {
                    await _lobbyApiInterface.RemovePlayerFromLobby(uasId, lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    PublishError(e);
                }
            }
            else
            {
                Debug.LogError("Only the host can remove other players from the lobby.");
            }
        }

        public async Task DeleteLobbyAsync(string lobbyId)
        {
            if (_localUser.IsHost)
            {
                try
                {
                    await _lobbyApiInterface.DeleteLobby(lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    PublishError(e);
                }
            }
            else
            {
                Debug.LogError("Only the host can delete a lobby.");
            }
        }

        /// <summary>
        /// Attempt to push a set of key-value pairs associated with the local player which will overwrite any existing data for these keys.
        /// </summary>
        public async Task UpdatePlayerDataAsync(Dictionary<string, PlayerDataObject> data)
        {
            if (!_rateLimitQuery.CanCall)
            {
                return;
            }

            try
            {
                var result = await _lobbyApiInterface.UpdatePlayer(CurrentUnityLobby.Id,
                    AuthenticationService.Instance.PlayerId, data, null, null);

                if (result != null)
                {
                    CurrentUnityLobby =
                        result; // Store the most up-to-date lobby now since we have it, instead of waiting for the next heartbeat.
                }
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    _rateLimitQuery.PutOnCooldown();
                }
                else if
                    (e.Reason != LobbyExceptionReason.LobbyNotFound &&
                     !_localUser
                         .IsHost) // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                {
                    PublishError(e);
                }
            }
        }

        /// <summary>
        /// Lobby can be provided info about Relay (or any other remote allocation) so it can add automatic disconnect handling.
        /// </summary>
        public async Task UpdatePlayerRelayInfoAsync(string allocationId, string connectionInfo)
        {
            if (!_rateLimitQuery.CanCall)
            {
                return;
            }

            try
            {
                await _lobbyApiInterface.UpdatePlayer(CurrentUnityLobby.Id, AuthenticationService.Instance.PlayerId,
                    new Dictionary<string, PlayerDataObject>(), allocationId, connectionInfo);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    _rateLimitQuery.PutOnCooldown();
                }
                else
                {
                    PublishError(e);
                }

                //todo - retry logic? SDK is supposed to handle this eventually
            }
        }

        /// <summary>
        /// Attempt to update a set of key-value pairs associated with a given lobby.
        /// </summary>
        public async Task UpdateLobbyDataAsync(Dictionary<string, DataObject> data)
        {
            if (!_rateLimitQuery.CanCall)
                return;

            Dictionary<string, DataObject> dataCurr = CurrentUnityLobby.Data ?? new Dictionary<string, DataObject>();

            foreach (KeyValuePair<string, DataObject> dataNew in data)
            {
                if (dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataNew.Value;
                else
                    dataCurr.Add(dataNew.Key, dataNew.Value);
            }

            // We would want to lock lobbies from appearing in queries if we're in relay mode
            // and the relay isn't fully set up yet.
            bool shouldLock = string.IsNullOrEmpty(_localLobby.RelayJoinCode);

            try
            {
                var result = await _lobbyApiInterface.UpdateLobby(CurrentUnityLobby.Id, dataCurr, shouldLock);

                if (result != null)
                    CurrentUnityLobby = result;
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                    _rateLimitQuery.PutOnCooldown();
                else
                    PublishError(e);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void UpdateLobby(float unused)
        {
            if (!_rateLimitQuery.CanCall)
                return;

            try
            {
                var lobby = await _lobbyApiInterface.GetLobby(_localLobby.LobbyID);

                CurrentUnityLobby = lobby;
                _localLobby.ApplyRemoteData(lobby);

                // As client, check if host is still in lobby.
                if (_localUser.IsHost)
                    return;

                foreach (KeyValuePair<string, LocalLobbyUser> lobbyUser in _localLobby.LobbyUsers)
                {
                    if (lobbyUser.Value.IsHost)
                        return;
                }

                //m_UnityServiceErrorMessagePub.Publish(new UnityServiceErrorMessage("Host left the lobby", "Disconnecting.", UnityServiceErrorMessage.Service.Lobby));
                await EndTracking();
                // No need to disconnect Netcode, it should already be handled by Netcode's callback to disconnect.
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    _rateLimitQuery.PutOnCooldown();
                }
                // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
                else if (e.Reason != LobbyExceptionReason.LobbyNotFound && !_localUser.IsHost)
                {
                    PublishError(e);
                }
            }
        }

        private void PublishError(LobbyServiceException e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})"; // Lobby error type, then HTTP error type.
            //m_UnityServiceErrorMessagePub.Publish(new UnityServiceErrorMessage("Lobby Error", reason, UnityServiceErrorMessage.Service.Lobby, e));
        }
    }
}