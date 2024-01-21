using GameCore.Enums;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class LoadLocationLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override InteractionType GetInteractionType() =>
            InteractionType.LoadLocationMobileLever;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        // protected override void OnLeverEnabled()
        // {
        //     ToggleInteract(canInteract: true);
        //     base.OnLeverEnabled();
        // }
    }
}