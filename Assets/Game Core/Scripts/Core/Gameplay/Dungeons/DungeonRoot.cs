using System;
using Cysharp.Threading.Tasks;
using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.LevelManager;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonRoot : MonoBehaviour, IDungeonCompleteReceiver
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDungeonsObserver dungeonsObserver, ILevelProviderObserver levelProviderObserver)
        {
            _dungeonsObserver = dungeonsObserver;
            _levelProviderObserver = levelProviderObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Floor Floor => _floor;

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<Floor> OnDungeonGenerationCompletedEvent = delegate { };
        
        private IDungeonsObserver _dungeonsObserver;
        private ILevelProviderObserver _levelProviderObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => RegisterDungeonRoot();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterDungeonRoot() =>
            _levelProviderObserver.RegisterDungeonRoot(_floor, dungeonRoot: this);

        private void CheckDungeon(Dungeon dungeon)
        {
            
        }

        private async void SendDungeonGenerationCompleted()
        {
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 1, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            _dungeonsObserver.DungeonGenerationCompleted(_floor);
            OnDungeonGenerationCompletedEvent.Invoke(_floor);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnDungeonComplete(Dungeon dungeon)
        {
            CheckDungeon(dungeon);
            SendDungeonGenerationCompleted();
        }
    }
}