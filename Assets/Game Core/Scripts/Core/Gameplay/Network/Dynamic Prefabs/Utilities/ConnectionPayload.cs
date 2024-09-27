using System;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    /// <summary>
    /// For the purposes of this sample, a hash representing the list of loaded network prefabs will be sent at a 
    /// connection request from a client to the server.
    /// </summary>
    [Serializable]
    public class ConnectionPayload
    {
        public int hashOfDynamicPrefabGUIDs;
    }
}