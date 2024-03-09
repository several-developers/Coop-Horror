using GameCore.Enums.Global;
using Unity.Netcode;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ConnectStatus ConnectStatus;
        public FixedPlayerName PlayerName;
    }
}