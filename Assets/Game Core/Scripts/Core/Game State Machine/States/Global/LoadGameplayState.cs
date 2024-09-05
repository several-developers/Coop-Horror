using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.StateMachine
{
    public class LoadGameplayState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadGameplayState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
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
            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;

            LoadGameplayScene();
        }

        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadGameplayScene() =>
            _scenesLoaderService.LoadScene(SceneName.Gameplay, isNetwork: true);

        private void EnterPrepareGameplaySceneState() =>
            _gameStateMachine.ChangeState<PrepareGameplaySceneState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSceneLoaded() => EnterPrepareGameplaySceneState();
    }
}