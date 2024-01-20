using GameCore.Gameplay.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
        }
    }
}