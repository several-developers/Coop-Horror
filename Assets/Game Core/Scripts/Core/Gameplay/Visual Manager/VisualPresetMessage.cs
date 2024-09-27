using GameCore.Enums.Gameplay;
using Unity.Netcode;

namespace GameCore.Gameplay.VisualManagement
{
    public struct VisualPresetMessage : INetworkSerializeByMemcpy
    {
        public VisualPresetType PresetType;
        public bool Instant;
    }
}