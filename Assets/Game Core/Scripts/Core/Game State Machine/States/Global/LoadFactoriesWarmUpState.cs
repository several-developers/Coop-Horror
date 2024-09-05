using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.StateMachine
{
    public class LoadFactoriesWarmUpState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadFactoriesWarmUpState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
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
            _scenesLoaderService.LoadScene(SceneName.FactoriesWarmUp, isNetwork: false);

            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;
        }
        
        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterFactoriesWarmUpState() =>
            _gameStateMachine.ChangeState<FactoriesWarmUpState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSceneLoaded() => EnterFactoriesWarmUpState();
    }
}