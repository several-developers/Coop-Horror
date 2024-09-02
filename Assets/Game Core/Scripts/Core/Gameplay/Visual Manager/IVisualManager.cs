using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.VisualManagement
{
    public interface IVisualManager
    {
        void ChangePreset(VisualPresetType presetType, bool instant = false);
        void SetLocationPreset(bool instant = false);
    }
}