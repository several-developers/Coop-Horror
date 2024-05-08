﻿using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public interface IRpcHandlerDecorator
    {
        event Action<DungeonsSeedData> OnGenerateDungeonsInnerEvent;
        event Action<Floor> OnStartElevatorInnerEvent;
        event Action<Floor> OnOpenElevatorInnerEvent;
        event Action<Floor, bool> OnTeleportToFireExitInnerEvent;

        void GenerateDungeons(DungeonsSeedData data);
        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
        void TeleportToFireExit(Floor floor, bool isInStairsLocation);
    }
}