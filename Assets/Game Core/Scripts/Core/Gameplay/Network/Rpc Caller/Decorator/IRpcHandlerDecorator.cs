﻿using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public interface IRpcHandlerDecorator
    {
        event Action<int, int> OnCreateItemPreviewInnerEvent;
        event Action<int> OnDestroyItemPreviewInnerEvent;
        event Action OnLocationLeftInnerEvent; // Mobile HQ left the location.
        event Action<DungeonsSeedData> OnGenerateDungeonsInnerEvent;
        event Action<Floor> OnStartElevatorInnerEvent;
        event Action<Floor> OnOpenElevatorInnerEvent;
        event Action<ulong, bool> OnTogglePlayerInsideMobileHQInnerEvent;
        event Action<Floor, bool> OnTeleportToFireExitInnerEvent;

        void CreateItemPreview(int slotIndex, int itemID);
        void DestroyItemPreview(int slotIndex);
        void LocationLeft();
        void GenerateDungeons(DungeonsSeedData data);
        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
        void TogglePlayerInsideMobileHQ(ulong clientID, bool isInside);
        void TeleportToFireExit(Floor floor, bool isInStairsLocation);
    }
}