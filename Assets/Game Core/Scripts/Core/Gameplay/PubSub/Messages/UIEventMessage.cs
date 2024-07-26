using GameCore.Enums.Gameplay;
using Unity.Netcode;

namespace GameCore.Gameplay.PubSub.Messages
{
    public struct UIEventMessage : INetworkSerializeByMemcpy
    {
        public UIEventType UIEventType;
    }
}