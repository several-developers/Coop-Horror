using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadMainMenuState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadMainMenuState(IGameStateMachine gameStateMachine, IScenesLoaderService2 scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService2 _scenesLoaderService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _scenesLoaderService.LoadScene(SceneName.MainMenu, isNetwork: false);

            _scenesLoaderService.OnSceneFinishedLoadingEvent += OnSceneFinishedLoading;
        }
        
        public void Exit() =>
            _scenesLoaderService.OnSceneFinishedLoadingEvent -= OnSceneFinishedLoading;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSceneFinishedLoading() => EnterPrepareMainMenuState();
    }
}