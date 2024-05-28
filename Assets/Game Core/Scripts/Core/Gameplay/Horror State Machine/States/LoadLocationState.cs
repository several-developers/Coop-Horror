using GameCore.Enums.Global;
using GameCore.Gameplay.Level.Locations;
using GameCore.Observers.Gameplay.Level;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LoadLocationState : IEnterState<SceneName>, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader,
            ILevelObserver levelObserver)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _levelObserver = levelObserver;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly ILevelObserver _levelObserver;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter(SceneName sceneName)
        {
            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;

            LoadLocation(sceneName);
        }

        public void Exit() =>
            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocation(SceneName sceneName) =>
            _locationsLoader.LoadLocationNetwork(sceneName);

        private void EnterGenerateDungeonsState() =>
            _horrorStateMachine.ChangeState<GenerateDungeonsState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLocationLoaded() => EnterGenerateDungeonsState();
    }
}