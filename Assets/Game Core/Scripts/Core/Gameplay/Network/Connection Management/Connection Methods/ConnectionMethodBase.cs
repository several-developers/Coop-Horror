using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public abstract class ConnectionMethodBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConnectionMethodBase(ConnectionManager connectionManager, string playerName)
        {
            ConnectionManager = connectionManager;
            PlayerName = playerName;
        }

        // FIELDS: --------------------------------------------------------------------------------

        protected ConnectionManager ConnectionManager;
        protected readonly string PlayerName;

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

        protected string GetPlayerId() =>
            AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : "";
    }
}