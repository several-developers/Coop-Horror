using GameCore.Enums.Gameplay;
using Unity.Netcode;

namespace GameCore.Gameplay.Managers.Visual
{
    public struct TargetVisualPresetMessage : INetworkSerializeByMemcpy
    {
        public VisualPresetType PresetType;
        public ulong ClientID;
        public bool Instant;
    }
}