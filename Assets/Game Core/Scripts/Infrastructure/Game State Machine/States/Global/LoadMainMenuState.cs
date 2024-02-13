using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadMainMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadMainMenuState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() =>
            _scenesLoaderService.LoadScene(SceneName.MainMenu, OnMainMenuSceneLoaded);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnMainMenuSceneLoaded() => EnterPrepareMainMenuState();
    }
}