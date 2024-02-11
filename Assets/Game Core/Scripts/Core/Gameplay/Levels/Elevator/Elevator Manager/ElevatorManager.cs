namespace GameCore.Gameplay.Levels.Elevator
{
    public class ElevatorManager : IElevatorManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorManager(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ILevelManager _levelManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
    }
}