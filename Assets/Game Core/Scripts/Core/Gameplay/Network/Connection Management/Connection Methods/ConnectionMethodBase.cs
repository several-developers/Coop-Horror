using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public abstract class ConnectionMethodBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConnectionMethodBase(ConnectionManager connectionManager, ProfileManager profileManager,
            string playerName)
        {
            ConnectionManager = connectionManager;
            _profileManager = profileManager;
            PlayerName = playerName;
        }

        // FIELDS: --------------------------------------------------------------------------------

        protected readonly ConnectionManager ConnectionManager;
        protected readonly string PlayerName;
        
        private readonly ProfileManager _profileManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract UniTask SetupHostConnectionAsync();

        public abstract UniTask SetupClientConnectionAsync();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void SetConnectionPayload(string playerId, string playerName)
        {
            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild
            });

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        protected string GetPlayerId()
        {
            // if (Services.Core.UnityServices.State != ServicesInitializationState.Initialized) 
            //     return GetGuid() + _profileManager.Profile;
            
            return AuthenticationService.Instance.IsSignedIn
                ? AuthenticationService.Instance.PlayerId
                : GetGuid() + _profileManager.Profile;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Either loads a Guid string from Unity preferences, or creates one and checkpoints it, then returns it.
        /// </summary>
        /// <returns>The Guid that uniquely identifies this client install, in string form. </returns>
        private static string GetGuid()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString();
            return guidString;
        }
    }
}