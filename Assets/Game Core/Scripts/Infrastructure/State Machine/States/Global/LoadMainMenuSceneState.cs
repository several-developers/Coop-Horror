using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadMainMenuSceneState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadMainMenuSceneState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _scenesLoaderService.LoadScene(SceneName.MainMenu, isNetwork: false);

            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;
        }
        
        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSceneLoaded() => EnterPrepareMainMenuState();
    }
}