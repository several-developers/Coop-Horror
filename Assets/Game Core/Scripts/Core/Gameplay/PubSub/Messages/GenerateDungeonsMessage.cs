using Unity.Netcode;

namespace GameCore.Gameplay.PubSub.Messages
{
    public struct GenerateDungeonsMessage : INetworkSerializeByMemcpy
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public int SeedOne;
        public int SeedTwo;
        public int SeedThree;
    }
}