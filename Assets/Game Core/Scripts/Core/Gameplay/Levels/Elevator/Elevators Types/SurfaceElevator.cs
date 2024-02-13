using GameCore.Gameplay.Network.Other;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class SurfaceElevator : ElevatorBase
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();
            AddSurfaceElevatorToLevelManager();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void AddSurfaceElevatorToLevelManager()
        {
            NetworkServiceLocator networkServiceLocator = NetworkServiceLocator.Get();
            ILevelManager levelManager = networkServiceLocator.GetLevelManager();
            levelManager.AddSurfaceElevator(surfaceElevator: this);
        }
    }
}