using Cysharp.Threading.Tasks;
using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels;
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

        // FIELDS: --------------------------------------------------------------------------------

        private IDungeonsObserver _dungeonsObserver;
        private ILevelProviderObserver _levelProviderObserver;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Floor Floor => _floor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddFireExitToLevelManager(FireExit fireExit)
        {
            bool isInStairsLocation = fireExit.IsInStairsLocation;

            if (isInStairsLocation)
                _levelProviderObserver.RegisterStairsFireExit(_floor, fireExit);
            else
                _levelProviderObserver.RegisterOtherFireExit(_floor, fireExit);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void SendDungeonGenerationCompleted()
        {
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 1, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            _dungeonsObserver.DungeonGenerationCompleted(_floor);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnDungeonComplete(Dungeon dungeon) => SendDungeonGenerationCompleted();
    }
}