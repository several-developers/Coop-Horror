using System;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
    }
}