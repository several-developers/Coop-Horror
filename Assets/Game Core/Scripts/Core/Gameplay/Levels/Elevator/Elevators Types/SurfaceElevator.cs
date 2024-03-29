using GameCore.Observers.Gameplay.LevelManager;
using Zenject;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class SurfaceElevator : ElevatorBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProviderObserver levelProviderObserver) =>
            _levelProviderObserver = levelProviderObserver;

        // FIELDS: --------------------------------------------------------------------------------
        
        private ILevelProviderObserver _levelProviderObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();
            RegisterSurfaceElevator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterSurfaceElevator() =>
            _levelProviderObserver.RegisterSurfaceElevator(surfaceElevator: this);
    }
}