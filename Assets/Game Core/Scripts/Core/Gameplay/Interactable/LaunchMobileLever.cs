using GameCore.Enums;

namespace GameCore.Gameplay.Interactable
{
    public class LaunchMobileLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override InteractionType GetInteractionType() =>
            InteractionType.LaunchMobile;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnLeverEnabled()
        {
            ToggleInteract(canInteract: true);
            base.OnLeverEnabled();
        }

        protected override void OnLeverDisabled()
        {
            ToggleInteract(canInteract: true);
            base.OnLeverDisabled();
        }
    }
}