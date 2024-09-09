using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using GameCore.Observers.Gameplay.Game;
using Object = UnityEngine.Object;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationServerState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationServerState(
            IHorrorStateMachine horrorStateMachine,
            ILocationsLoader locationsLoader,
            ILevelProvider levelProvider,
            IItemsProvider itemsProvider,
            IGameManagerDecorator gameManagerDecorator,
            IGameObserver gameObserver
        )
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _levelProvider = levelProvider;
            _itemsProvider = itemsProvider;
            _gameManagerDecorator = gameManagerDecorator;
            _gameObserver = gameObserver;
            _cancellationTokenSource = new CancellationTokenSource();

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly ILevelProvider _levelProvider;
        private readonly IItemsProvider _itemsProvider;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IGameObserver _gameObserver;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _cancellationTokenSource?.Dispose();

        public async UniTaskVoid Enter()
        {
            LocationName previousLocation = _gameManagerDecorator.GetPreviousLocation();
            _gameObserver.TrainArrivedAtBase(previousLocation);

            // TEMP
            bool isCanceled = await UniTask
                .Delay(millisecondsDelay: 500, cancellationToken: _cancellationTokenSource.Token)
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            DestroyAllItems();
            KillAllMonsters();
            ClearDungeonElevators();
            UnloadLastLocation();
            EnterLeaveLocationClientState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DestroyAllItems()
        {
            IReadOnlyDictionary<int, ItemObjectBase> allItems = _itemsProvider.GetAllItems();
            var allKeys = new List<int>(allItems.Keys);

            foreach (KeyValuePair<int, ItemObjectBase> pair in allItems)
            {
                ItemObjectBase itemObject = pair.Value;
                bool destroyOnSceneUnload = itemObject.DestroyOnSceneUnload;

                if (!destroyOnSceneUnload)
                {
                    int key = pair.Key;
                    allKeys.Remove(key);
                    continue;
                }

                Object.Destroy(itemObject.gameObject);
            }

            foreach (int uniqueItemID in allKeys)
                _itemsProvider.RemoveItem(uniqueItemID);
        }

        private void KillAllMonsters()
        {
        }

        private void ClearDungeonElevators()
        {
            List<Floor> floors = new() { Floor.One, Floor.Two, Floor.Three };

            foreach (Floor floor in floors)
            {
                bool isElevatorFound = TryGetElevator(floor, out ElevatorBase elevatorBase);

                if (!isElevatorFound)
                    continue;

                Object.Destroy(elevatorBase.gameObject);
            }

            // LOCAL METHODS: -----------------------------

            bool TryGetElevator(Floor floor, out ElevatorBase result) =>
                _levelProvider.TryGetElevator(floor, out result);
        }

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastScene();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();
    }
}