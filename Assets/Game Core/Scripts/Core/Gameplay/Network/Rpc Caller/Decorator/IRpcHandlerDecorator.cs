﻿using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public interface IRpcHandlerDecorator
    {
        event Action<int, int> OnCreateItemPreviewEvent;
        event Action<int> OnDestroyItemPreviewEvent;
        event Action<SceneName> OnLoadLocationEvent;
        event Action OnStartLeavingLocationEvent;
        event Action OnLeaveLocationEvent;
        event Action OnLocationLoadedEvent;
        event Action OnLeftLocationEvent;
        event Action<DungeonsSeedData> OnGenerateDungeonsEvent;
        event Action<Floor> OnStartElevatorEvent;
        event Action<Floor> OnOpenElevatorEvent;
        event Action<ulong, bool> OnTogglePlayerInsideMobileHQEvent;
        event Action<Floor, bool> OnTeleportToFireExitEvent;

        void CreateItemPreview(int slotIndex, int itemID);
        void DestroyItemPreview(int slotIndex);
        void LoadLocation(SceneName sceneName);
        void StartLeavingLocation();
        void LeaveLocation();
        void LocationLoaded();
        void LeftLocation();
        void GenerateDungeons(DungeonsSeedData data);
        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
        void TogglePlayerInsideMobileHQ(ulong clientID, bool isInside);
        void TeleportToFireExit(Floor floor, bool isInStairsLocation);
    }
}