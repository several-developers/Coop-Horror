using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Rpc;
using Sirenix.OdinInspector;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonsManager : IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DungeonsManager(ILevelProvider levelProvider, IDungeonsObserver dungeonsObserver,
            IRpcObserver rpcObserver)
        {
            _levelProvider = levelProvider;
            _dungeonsObserver = dungeonsObserver;
            _rpcObserver = rpcObserver;
            _dungeonsList = new List<DungeonWrapper>(capacity: 3);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ILevelProvider _levelProvider;
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly IRpcObserver _rpcObserver;
        private readonly List<DungeonWrapper> _dungeonsList;

        private int _generatedDungeonsAmount;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            GetDungeons();
            SetDungeonsRoots();

            _rpcObserver.OnGenerateDungeonsEvent += OnGenerateDungeons;

            _dungeonsObserver.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;
        }

        public void Dispose()
        {
            _rpcObserver.OnGenerateDungeonsEvent -= OnGenerateDungeons;

            _dungeonsObserver.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;
        }

        public void ClearDungeons()
        {
            _generatedDungeonsAmount = 0;

            foreach (DungeonWrapper dungeonWrapper in _dungeonsList)
                dungeonWrapper.Clear();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GetDungeons()
        {
            Floor[] floors = { Floor.One, Floor.Two, Floor.Three };

            foreach (Floor floor in floors)
            {
                bool isDungeonFound = _levelProvider.TryGetDungeon(floor, out DungeonWrapper dungeonWrapper);

                if (!isDungeonFound)
                {
                    Log.PrintError(log: $"Dungeon with floor <gb>{floor}</gb> <rb>not found</rb>!");
                    continue;
                }

                _dungeonsList.Add(dungeonWrapper);
            }
        }

        private void SetDungeonsRoots()
        {
            foreach (DungeonWrapper dungeonWrapper in _dungeonsList)
            {
                Floor floor = dungeonWrapper.GetFloor();
                bool isDungeonRootFound = _levelProvider.TryGetDungeonRoot(floor, out DungeonRoot dungeonRoot);

                if (!isDungeonRootFound)
                {
                    Log.PrintError(log: $"Dungeon Root <gb>{floor}</gb> <rb>not found</rb>!");
                    return;
                }

                dungeonWrapper.SetRoot(dungeonRoot.gameObject);
            }
        }

        private void GenerateDungeons()
        {
            _generatedDungeonsAmount = 0;

            foreach (DungeonWrapper dungeonWrapper in _dungeonsList)
                dungeonWrapper.Generate();
        }

        private void GenerateDungeonsWithSeed(DungeonsSeedData data)
        {
            _generatedDungeonsAmount = 0;

            foreach (DungeonWrapper dungeonWrapper in _dungeonsList)
            {
                Floor floor = dungeonWrapper.GetFloor();
                int seed;

                switch (floor)
                {
                    case Floor.One:
                        seed = data.SeedOne;
                        break;

                    case Floor.Two:
                        seed = data.SeedTwo;
                        break;

                    case Floor.Three:
                        seed = data.SeedThree;
                        break;

                    default:
                        Log.PrintError(log: "Wrong floor setup!");
                        continue;
                }

                dungeonWrapper.GenerateWithSeed(seed);
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonGenerationCompleted(Floor floor)
        {
            _generatedDungeonsAmount++;

            if (_generatedDungeonsAmount >= 3)
                _dungeonsObserver.DungeonsGenerationCompleted();
        }

        private void OnGenerateDungeons(DungeonsSeedData data) => GenerateDungeonsWithSeed(data);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

#warning ПЕРЕНЕСТИ
        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugGenerateDungeons() => GenerateDungeons();

        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugGenerateDungeonsWithSeed(int seedOne, int seedTwo, int seedThree)
        {
            DungeonsSeedData data = new(seedOne, seedTwo, seedThree);
            GenerateDungeonsWithSeed(data);
        }

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugClearDungeons() => ClearDungeons();
    }
}