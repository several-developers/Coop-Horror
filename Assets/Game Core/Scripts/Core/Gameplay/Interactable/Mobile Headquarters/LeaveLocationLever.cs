using GameCore.Enums;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class LeaveLocationLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override InteractionType GetInteractionType() =>
            InteractionType.LeaveLocationMobileLever;
    }
}