using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class LoadLocationLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override InteractionType GetInteractionType() =>
            InteractionType.LoadLocationMobileLever;
    }
}