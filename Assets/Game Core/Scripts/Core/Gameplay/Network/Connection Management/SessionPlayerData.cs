using GameCore.Gameplay.Network.Other;
using GameCore.Gameplay.Network.SessionManagement;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public SessionPlayerData(ulong clientID, string name, NetworkGuid avatarNetworkGuid, int currentHitPoints = 0,
            bool isConnected = false, bool hasCharacterSpawned = false)
        {
            ClientID = clientID;
            PlayerName = name;
            PlayerNumber = -1;
            PlayerPosition = Vector3.zero;
            PlayerRotation = Quaternion.identity;
            AvatarNetworkGuid = avatarNetworkGuid;
            CurrentHitPoints = currentHitPoints;
            IsConnected = isConnected;
            HasCharacterSpawned = hasCharacterSpawned;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        // FIELDS: --------------------------------------------------------------------------------
        
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        
        /// Instead of using a NetworkGuid (two ulongs) we could just use an int or even a byte-sized index
        /// into an array of possible avatars defined in our game data source.
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Reinitialize() =>
            HasCharacterSpawned = false;
    }
}