using GameCore.Observers.Gameplay.LevelManager;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
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

        protected override void StartAll() => RegisterSurfaceElevator();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterSurfaceElevator() =>
            _levelProviderObserver.RegisterSurfaceElevator(surfaceElevator: this);
    }
}