using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.MainMenu.LobbiesMenu.RelayLobby;

namespace GameCore.StateMachine
{
    public class CreateRelayLobbyState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateRelayLobbyState(
            IGameStateMachine gameStateMachine,
            IScenesLoaderService scenesLoaderService,
            IMenusFactory menusFactory
        )
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;
            _menusFactory = menusFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;
        private readonly IMenusFactory _menusFactory;

        private RelayLobbyMenuView _relayLobbyMenu;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;

            CreateRelayLobbyMenu();
        }

        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void CreateRelayLobbyMenu()
        {
            // _relayLobbyMenu = MenuFactory.Create<RelayLobbyMenuView>();
            _relayLobbyMenu = await _menusFactory.Create<RelayLobbyMenuView>();

            _relayLobbyMenu.OnCloseClickedEvent += OnCloseClicked;
        }

        private void EnterPrepareGameplaySceneState() =>
            _gameStateMachine.ChangeState<PrepareGameplaySceneState>();

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => EnterMainMenuState();

        private void OnSceneLoaded() => EnterPrepareGameplaySceneState();
    }
}