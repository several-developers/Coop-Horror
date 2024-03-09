using GameCore.Enums.Global;
using GameCore.Gameplay.Network;
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

        private void LoadGameplayScene()
        {
            TheNetworkHorror network = TheNetworkHorror.Get();
            bool isMultiplayerEnabled = network.IsActive();
            
            if (isMultiplayerEnabled)
                _scenesLoaderService.LoadSceneNetwork(SceneName.Gameplay, OnGameplaySceneLoaded);
            else
                _scenesLoaderService.LoadScene(SceneName.Gameplay, OnGameplaySceneLoaded);
        }
        
        private void EnterGameplayState() =>
            _gameStateMachine.ChangeState<GameplayState>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnGameplaySceneLoaded()
        {
            TheNetworkHorror networkHorror = TheNetworkHorror.Get();
            networkHorror.StartSmth();
            
            EnterGameplayState();
        }
    }
}