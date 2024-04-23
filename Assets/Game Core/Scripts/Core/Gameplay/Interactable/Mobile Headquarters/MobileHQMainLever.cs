using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class MobileHQMainLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override InteractionType GetInteractionType() =>
            InteractionType.MobileHQMainLever;
    }
}