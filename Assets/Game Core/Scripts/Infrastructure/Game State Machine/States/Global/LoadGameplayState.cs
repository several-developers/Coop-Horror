using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadGameplayState : IEnterState
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

        public void Enter() => LoadGameplayScene();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadGameplayScene() =>
            _scenesLoaderService.LoadSceneNetwork(SceneName.Gameplay, OnGameplaySceneLoaded);

        private void EnterGameplaySceneState() =>
            _gameStateMachine.ChangeState<GameplaySceneState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnGameplaySceneLoaded() => EnterGameplaySceneState();
    }
}