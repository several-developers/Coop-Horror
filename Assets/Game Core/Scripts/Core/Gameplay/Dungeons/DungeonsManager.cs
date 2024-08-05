using System;
using System.Collections.Generic;
using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using GameCore.Observers.Gameplay.Dungeons;
using Sirenix.OdinInspector;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public interface IDungeonsManager
    {
        void GenerateDungeonsOnAllClients();
    }

    public class DungeonsManager : IDungeonsManager, IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DungeonsManager(
            ILevelProvider levelProvider,
            IDungeonsObserver dungeonsObserver,
            ISubscriber<GenerateDungeonsMessage> generateDungeonsMessageSubscriber,
            IPublisher<GenerateDungeonsMessage> generateDungeonsMessagePublisher
        )
        {
            _levelProvider = levelProvider;
            _dungeonsObserver = dungeonsObserver;
            _generateDungeonsMessageSubscriber = generateDungeonsMessageSubscriber;
            _generateDungeonsMessagePublisher = generateDungeonsMessagePublisher;
            _dungeonsList = new List<DungeonWrapper>(capacity: 3);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ILevelProvider _levelProvider;
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly ISubscriber<GenerateDungeonsMessage> _generateDungeonsMessageSubscriber;
        private readonly IPublisher<GenerateDungeonsMessage> _generateDungeonsMessagePublisher;
        private readonly List<DungeonWrapper> _dungeonsList;

        private int _generatedDungeonsAmount;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            GetDungeons();
            SetDungeonsRoots();

            _generateDungeonsMessageSubscriber.Subscribe(OnGenerateDungeonsMessageReceived);

            _dungeonsObserver.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;
        }

        public void Dispose()
        {
            _generateDungeonsMessageSubscriber.Unsubscribe(OnGenerateDungeonsMessageReceived);

            _dungeonsObserver.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;
        }

        public void GenerateDungeonsOnAllClients()
        {
            RandomStream randomStream = new();
            int seedOne = GenerateRandomSeed();
            int seedTwo = GenerateRandomSeed();
            int seedThree = GenerateRandomSeed();

            GenerateDungeonsMessage message = new()
            {
                SeedOne = seedOne,
                SeedTwo = seedTwo,
                SeedThree = seedThree
            };

            _generateDungeonsMessagePublisher.Publish(message);

            // LOCAL METHODS: -----------------------------

            int GenerateRandomSeed() =>
                randomStream.Next();
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

        private void GenerateDungeonsWithSeed(GenerateDungeonsMessage message)
        {
            _generatedDungeonsAmount = 0;

            foreach (DungeonWrapper dungeonWrapper in _dungeonsList)
            {
                Floor floor = dungeonWrapper.GetFloor();
                int seed;

                switch (floor)
                {
                    case Floor.One:
                        seed = message.SeedOne;
                        break;

                    case Floor.Two:
                        seed = message.SeedTwo;
                        break;

                    case Floor.Three:
                        seed = message.SeedThree;
                        break;

                    default:
                        Log.PrintError(log: "Wrong floor setup!");
                        continue;
                }

                dungeonWrapper.GenerateWithSeed(seed);
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGenerateDungeonsMessageReceived(GenerateDungeonsMessage message) =>
            GenerateDungeonsWithSeed(message);

        private void OnDungeonGenerationCompleted(Floor floor)
        {
            _generatedDungeonsAmount++;

            if (_generatedDungeonsAmount >= 3)
                _dungeonsObserver.DungeonsGenerationCompleted();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

#warning ПЕРЕНЕСТИ
        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugGenerateDungeons() => GenerateDungeons();

        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugGenerateDungeonsWithSeed(int seedOne, int seedTwo, int seedThree)
        {
            GenerateDungeonsMessage message = new()
            {
                SeedOne = seedOne,
                SeedTwo = seedTwo,
                SeedThree = seedThree
            };
            
            GenerateDungeonsWithSeed(message);
        }

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugClearDungeons() => ClearDungeons();
    }
}