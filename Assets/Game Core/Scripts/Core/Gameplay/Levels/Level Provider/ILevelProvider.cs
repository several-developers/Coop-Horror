﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Gameplay.Levels
{
    public interface ILevelProvider
    {
        void Clear();
        bool TryGetElevator(Floor floor, out ElevatorBase elevator);
        bool TryGetStairsFireExit(Floor floor, out FireExit fireExit);
        bool TryGetOtherFireExit(Floor floor, out FireExit fireExit);
        bool TryGetDungeon(Floor floor, out DungeonWrapper dungeonWrapper);
        bool TryGetDungeonRoot(Floor floor, out DungeonRoot dungeonRoot);
    }
}