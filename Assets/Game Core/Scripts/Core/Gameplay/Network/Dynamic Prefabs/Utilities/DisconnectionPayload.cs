using System;
using System.Collections.Generic;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    /// <summary>
    /// For the purposes of this sample, this class will be serialized inside of Netcode for GameObject's
    /// DisconnectReason string, and sent to clients which failed to connect. It contains the reason enum, and the list
    /// of Addressable GUIDs to load before reconnecting.
    /// </summary>
    [Serializable]
    public class DisconnectionPayload
    {
        public DisconnectReason reason;
        public List<string> guids;
    }
}