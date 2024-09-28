using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Managers.Visual
{
    public interface IVisualManager
    {
        void ChangePreset(VisualPresetType presetType, bool instant = false);
        void ChangePresetForAll(VisualPresetType presetType, bool instant = false);
        void ChangePresetByFloor(Floor floor, ulong clientID, bool instant = false);
        void SetLocationPreset(bool instant = false);
        void SetLocationPresetForAll(bool instant = false);
    }
}