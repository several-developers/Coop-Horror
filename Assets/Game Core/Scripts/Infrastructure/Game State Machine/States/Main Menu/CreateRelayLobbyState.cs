using GameCore.Gameplay.Factories;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.MainMenu.LobbiesMenu.RelayLobby;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateRelayLobbyState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateRelayLobbyState(IGameStateMachine gameStateMachine, IScenesLoaderService2 scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService2 _scenesLoaderService;

        private RelayLobbyMenuView _relayLobbyMenu;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateRelayLobbyMenu();

            _scenesLoaderService.OnSceneFinishedLoadingEvent += OnSceneFinishedLoading;
        }

        public void Exit() =>
            _scenesLoaderService.OnSceneFinishedLoadingEvent -= OnSceneFinishedLoading;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void CreateRelayLobbyMenu()
        {
            _relayLobbyMenu = MenuFactory.Create<RelayLobbyMenuView>();

            _relayLobbyMenu.OnCloseClickedEvent += OnCloseClicked;
        }

        private void EnterGameplaySceneState() =>
            _gameStateMachine.ChangeState<GameplaySceneState>();

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => EnterMainMenuState();

        private void OnSceneFinishedLoading() => EnterGameplaySceneState();
    }
}