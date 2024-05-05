using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items.Rigging;

namespace GameCore.Infrastructure.Providers.Gameplay.RigPresets
{
    public interface IRigPresetsProvider
    {
        bool TryGetPresetMeta(RigPresetType presetType, out RigPresetMeta presetMeta);
    }
}