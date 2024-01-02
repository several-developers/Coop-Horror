﻿using GameCore.Enums;

namespace GameCore.Gameplay.Interactable
{
    public class ToggleMobileDoorLever : LeverBase
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override InteractionType GetInteractionType() =>
            InteractionType.ToggleMobileDoor;
    }
}