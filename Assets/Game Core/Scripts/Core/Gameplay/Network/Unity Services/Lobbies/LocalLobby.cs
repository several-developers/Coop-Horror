using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace GameCore.Gameplay.Network.UnityServices.Lobbies
{
    /// <summary>
    /// A local wrapper around a lobby's remote data, with additional functionality for providing
    /// that data to UI elements and tracking local player objects.
    /// </summary>
    [Serializable]
    public sealed class LocalLobby
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public Dictionary<string, LocalLobbyUser> LobbyUsers { get; private set; } = new();
        public LobbyData Data => new(_lobbyData);

        public string LobbyID
        {
            get => _lobbyData.LobbyID;
            set
            {
                _lobbyData.LobbyID = value;
                SendChanged();
            }
        }

        public string LobbyName
        {
            get => _lobbyData.LobbyName;
            set
            {
                _lobbyData.LobbyName = value;
                SendChanged();
            }
        }

        public string LobbyCode
        {
            get => _lobbyData.LobbyCode;
            set
            {
                _lobbyData.LobbyCode = value;
                SendChanged();
            }
        }

        public string RelayJoinCode
        {
            get => _lobbyData.RelayJoinCode;
            set
            {
                _lobbyData.RelayJoinCode = value;
                SendChanged();
            }
        }

        public int MaxPlayerCount
        {
            get => _lobbyData.MaxPlayerCount;
            set
            {
                _lobbyData.MaxPlayerCount = value;
                SendChanged();
            }
        }

        public int PlayerCount => LobbyUsers.Count;

        public bool Private
        {
            get => _lobbyData.Private;
            set
            {
                _lobbyData.Private = value;
                SendChanged();
            }
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<LocalLobby> OnChangedEvent;

        private LobbyData _lobbyData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddUser(LocalLobbyUser user)
        {
            if (LobbyUsers.ContainsKey(user.ID))
                return;

            DoAddUser(user);
            SendChanged();
        }

        public void RemoveUser(LocalLobbyUser user)
        {
            DoRemoveUser(user);
            SendChanged();
        }

        public void CopyDataFrom(LobbyData data, Dictionary<string, LocalLobbyUser> currentUsers)
        {
            _lobbyData = data;

            if (currentUsers == null)
            {
                LobbyUsers = new Dictionary<string, LocalLobbyUser>();
            }
            else
            {
                List<LocalLobbyUser> toRemove = new();

                foreach (KeyValuePair<string, LocalLobbyUser> oldUser in LobbyUsers)
                {
                    if (currentUsers.ContainsKey(oldUser.Key))
                        oldUser.Value.CopyDataFrom(currentUsers[oldUser.Key]);
                    else
                        toRemove.Add(oldUser.Value);
                }

                foreach (LocalLobbyUser remove in toRemove)
                    DoRemoveUser(remove);

                foreach (KeyValuePair<string, LocalLobbyUser> currUser in currentUsers)
                {
                    if (!LobbyUsers.ContainsKey(currUser.Key))
                        DoAddUser(currUser.Value);
                }
            }

            SendChanged();
        }
        
        public void ApplyRemoteData(Lobby lobby)
        {
            // Technically, this is largely redundant after the first assignment,
            // but it won't do any harm to assign it again.
            var info = new LobbyData();
            info.LobbyID = lobby.Id;
            info.LobbyCode = lobby.LobbyCode;
            info.Private = lobby.IsPrivate;
            info.LobbyName = lobby.Name;
            info.MaxPlayerCount = lobby.MaxPlayers;

            if (lobby.Data != null)
            {
                // By providing RelayCode through the lobby data with Member visibility,
                // we ensure a client is connected to the lobby before they could attempt a relay connection,
                // preventing timing issues between them.
                info.RelayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null;
            }
            else
            {
                info.RelayJoinCode = null;
            }

            Dictionary<string, LocalLobbyUser> lobbyUsers = new();

            foreach (Player player in lobby.Players)
            {
                if (player.Data != null)
                {
                    if (LobbyUsers.ContainsKey(player.Id))
                    {
                        lobbyUsers.Add(player.Id, LobbyUsers[player.Id]);
                        continue;
                    }
                }

                // If the player isn't connected to Relay, get the most recent data that the lobby knows.
                // (If we haven't seen this player yet, a new local representation of the player will have already been added by the LocalLobby.)
                var incomingData = new LocalLobbyUser
                {
                    IsHost = lobby.HostId.Equals(player.Id),
                    DisplayName = player.Data?.ContainsKey("DisplayName") == true
                        ? player.Data["DisplayName"].Value
                        : default,
                    ID = player.Id
                };

                lobbyUsers.Add(incomingData.ID, incomingData);
            }

            CopyDataFrom(info, lobbyUsers);
        }

        public void Reset(LocalLobbyUser localUser)
        {
            CopyDataFrom(new LobbyData(), currentUsers: new Dictionary<string, LocalLobbyUser>());
            AddUser(localUser);
        }

        public Dictionary<string, DataObject> GetDataForUnityServices()
        {
            return new Dictionary<string, DataObject>
            {
                { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, RelayJoinCode) }
            };
        }

        /// <summary>
        /// Create a list of new LocalLobbies from the result of a lobby list query.
        /// </summary>
        public static List<LocalLobby> CreateLocalLobbies(QueryResponse response)
        {
            List<LocalLobby> retLst = new();

            foreach (Lobby lobby in response.Results)
                retLst.Add(Create(lobby));

            return retLst;
        }

        public static LocalLobby Create(Lobby lobby)
        {
            var data = new LocalLobby();
            data.ApplyRemoteData(lobby);
            return data;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DoAddUser(LocalLobbyUser user)
        {
            LobbyUsers.Add(user.ID, user);
            user.OnChangedEvent += OnChangedUser;
        }

        private void DoRemoveUser(LocalLobbyUser user)
        {
            if (!LobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"Player {user.DisplayName}({user.ID}) does not exist in lobby: {LobbyID}");
                return;
            }

            LobbyUsers.Remove(user.ID);
            user.OnChangedEvent -= OnChangedUser;
        }

        private void SendChanged() =>
            OnChangedEvent?.Invoke(this);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnChangedUser(LocalLobbyUser user) => SendChanged();

        // OTHER: ---------------------------------------------------------------------------------

        public struct LobbyData
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public LobbyData(LobbyData existing)
            {
                LobbyID = existing.LobbyID;
                LobbyCode = existing.LobbyCode;
                RelayJoinCode = existing.RelayJoinCode;
                LobbyName = existing.LobbyName;
                Private = existing.Private;
                MaxPlayerCount = existing.MaxPlayerCount;
            }

            public LobbyData(string lobbyCode)
            {
                LobbyID = null;
                LobbyCode = lobbyCode;
                RelayJoinCode = null;
                LobbyName = null;
                Private = false;
                MaxPlayerCount = -1;
            }

            // PROPERTIES: ----------------------------------------------------------------------------

            public string LobbyID { get; set; }
            public string LobbyCode { get; set; }
            public string RelayJoinCode { get; set; }
            public string LobbyName { get; set; }
            public bool Private { get; set; }
            public int MaxPlayerCount { get; set; }
        }
    }
}