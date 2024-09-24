using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Level.Elevator;

namespace GameCore.Gameplay.Level
{
    public interface ILevelProvider
    {
        void ClearLevel();
        bool TryGetStairsFireExit(Floor floor, out FireExit fireExit);
        bool TryGetOtherFireExit(Floor floor, out FireExit fireExit);
        bool TryGetDungeon(Floor floor, out DungeonWrapper dungeonWrapper);
        bool TryGetDungeonRoot(Floor floor, out DungeonRoot dungeonRoot);
    }
}