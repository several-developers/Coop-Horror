using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Level.Elevator;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Storages.Entities;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using GameCore.Observers.Gameplay.Game;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationServerState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationServerState(
            IHorrorStateMachine horrorStateMachine,
            ILocationsLoader locationsLoader,
            IItemsProvider itemsProvider,
            IEntitiesStorage entitiesStorage,
            IGameManagerDecorator gameManagerDecorator,
            IGameObserver gameObserver
        )
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _itemsProvider = itemsProvider;
            _entitiesStorage = entitiesStorage;
            _gameManagerDecorator = gameManagerDecorator;
            _gameObserver = gameObserver;
            _cancellationTokenSource = new CancellationTokenSource();

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly IItemsProvider _itemsProvider;
        private readonly IEntitiesStorage _entitiesStorage;
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
            KillAllEntities();
            ResetElevator();
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
                bool isPickedUp = itemObject.IsPickedUp();
                bool skipDestruction = !destroyOnSceneUnload || isPickedUp;

                if (skipDestruction)
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

        private void KillAllEntities()
        {
            IEnumerable<GameObject> allEntities = _entitiesStorage.GetAllEntities();

            foreach (GameObject entityGameObject in allEntities)
            {
                if (entityGameObject == null)
                    continue;
                
                Object.Destroy(entityGameObject);
            }
            
            _entitiesStorage.Clear();
        }

        private static void ResetElevator()
        {
            ElevatorEntity elevatorEntity = ElevatorEntity.Get();
            elevatorEntity.ResetElevator();
        }

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastScene();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();
    }
}