using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class ToggleMobileDoorLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override InteractionType GetInteractionType() =>
            InteractionType.ToggleMobileDoor;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnLeverEnabled()
        {
            base.OnLeverEnabled();
        }

        protected override void OnLeverDisabled()
        {
            base.OnLeverDisabled();
        }
    }
}