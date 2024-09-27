using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadAssetsWarmUpSceneState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadAssetsWarmUpSceneState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
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
            _scenesLoaderService.LoadScene(SceneName.AssetsWarmUp, isNetwork: false);

            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;
        }
        
        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterAssetsWarmUpState() =>
            _gameStateMachine.ChangeState<AssetsWarmUpState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSceneLoaded() => EnterAssetsWarmUpState();
    }
}