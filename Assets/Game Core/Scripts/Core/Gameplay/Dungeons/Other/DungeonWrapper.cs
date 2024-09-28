using DunGen;
using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonWrapper
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DungeonWrapper(RuntimeDungeon runtimeDungeon, Floor floor)
        {
            _runtimeDungeon = runtimeDungeon;
            _floor = floor;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly RuntimeDungeon _runtimeDungeon;
        private readonly Floor _floor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Generate() =>
            _runtimeDungeon.Generate();
        
        public void GenerateWithSeed(int seed) =>
            _runtimeDungeon.GenerateWithSeed(seed);

        public void SetRoot(GameObject root) =>
            _runtimeDungeon.Root = root;

        public void Clear() =>
            _runtimeDungeon.Clear();
        
        public Floor GetFloor() => _floor;
    }
}