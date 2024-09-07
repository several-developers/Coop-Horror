using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
#warning УБРАТЬ?
    public class LoadLocationState : IEnterState<LocationName>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter(LocationName locationName)
        {
            LoadLocation(locationName);
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocation(LocationName locationName) =>
            _locationsLoader.LoadLocation(locationName);

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}