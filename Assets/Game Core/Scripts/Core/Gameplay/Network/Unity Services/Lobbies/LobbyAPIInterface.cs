using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace GameCore.Gameplay.Network.UnityServices.Lobbies
{
    /// <summary>
    /// Wrapper for all the interactions with the Lobby API.
    /// </summary>
    public class LobbyAPIInterface
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LobbyAPIInterface()
        {
            // Filter for open lobbies only
            _filters = new List<QueryFilter>
            {
                new(field: QueryFilter.FieldOptions.AvailableSlots, op: QueryFilter.OpOptions.GT, value: "0")
            };

            // Order by newest lobbies first
            _order = new List<QueryOrder>
            {
                new(asc: false, field: QueryOrder.FieldOptions.Created)
            };
        }

        // FIELDS: --------------------------------------------------------------------------------

        // If more are necessary, consider retrieving paginated results or using filters.
        private const int MaxLobbiesToShow = 16;

        private readonly List<QueryFilter> _filters;
        private readonly List<QueryOrder> _order;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async Task<Lobby> CreateLobby(string requesterUasId, string lobbyName, int maxPlayers, bool isPrivate,
            Dictionary<string, PlayerDataObject> hostUserData, Dictionary<string, DataObject> lobbyData)
        {
            CreateLobbyOptions createOptions = new()
            {
                IsPrivate = isPrivate,
                Player = new Player(id: requesterUasId, data: hostUserData),
                Data = lobbyData
            };

            return await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        }

        public async Task DeleteLobby(string lobbyId) =>
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);

        public async Task<Lobby> JoinLobbyByCode(string requesterUasId, string lobbyCode,
            Dictionary<string, PlayerDataObject> localUserData)
        {
            JoinLobbyByCodeOptions joinOptions = new()
                { Player = new Player(id: requesterUasId, data: localUserData) };

            return await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
        }

        public async Task<Lobby> JoinLobbyById(string requesterUasId, string lobbyId,
            Dictionary<string, PlayerDataObject> localUserData)
        {
            JoinLobbyByIdOptions joinOptions = new()
                { Player = new Player(id: requesterUasId, data: localUserData) };

            return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
        }

        public async Task<Lobby> QuickJoinLobby(string requesterUasId,
            Dictionary<string, PlayerDataObject> localUserData)
        {
            QuickJoinLobbyOptions joinRequest = new()
            {
                Filter = _filters,
                Player = new Player(id: requesterUasId, data: localUserData)
            };

            return await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);
        }

        public async Task<Lobby> ReconnectToLobby(string lobbyId) =>
            await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);

        public async Task RemovePlayerFromLobby(string requesterUasId, string lobbyId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, requesterUasId);
            }
            catch (LobbyServiceException e) when (e is { Reason: LobbyExceptionReason.PlayerNotFound })
            {
                // If Player is not found, they have already left the lobby or have been kicked out. No need to throw here
            }
        }

        public async Task<QueryResponse> QueryAllLobbies()
        {
            QueryLobbiesOptions queryOptions = new()
            {
                Count = MaxLobbiesToShow,
                Filters = _filters,
                Order = _order
            };

            return await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        }

        public async Task<Lobby> GetLobby(string lobbyId) =>
            await LobbyService.Instance.GetLobbyAsync(lobbyId);

        public async Task<Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock)
        {
            UpdateLobbyOptions updateOptions = new() { Data = data, IsLocked = shouldLock };
            return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }

        public async Task<Lobby> UpdatePlayer(string lobbyId, string playerId,
            Dictionary<string, PlayerDataObject> data, string allocationId, string connectionInfo)
        {
            UpdatePlayerOptions updateOptions = new()
            {
                Data = data,
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            
            return await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions);
        }

        public async void SendHeartbeatPing(string lobbyId) =>
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
    }
}