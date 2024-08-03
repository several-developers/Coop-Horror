using GameCore.Enums.Gameplay;
using Unity.Netcode;

namespace GameCore.Gameplay.PubSub.Messages
{
    public struct UIEventMessage : INetworkSerializeByMemcpy
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public UIEventType UIEventType;
    }
}