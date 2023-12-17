using GameCore.Enums;
using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.MainMenu.CreateLobbyMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;
using Unity.Netcode;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateLobbyState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateLobbyState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;

        private CreateLobbyMenuView _createLobbyMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateCreateLobbyMenu();
            CreateSaveSelectionMenuView();

            _createLobbyMenuView.OnStartGameClickedEvent += OnStartGameClicked;
        }

        public void Exit() =>
            _createLobbyMenuView.OnStartGameClickedEvent -= OnStartGameClicked;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateCreateLobbyMenu() =>
            _createLobbyMenuView = MenuFactory.Create<CreateLobbyMenuView>();

        private void StartHost() =>
            NetworkManager.Singleton.StartHost();

        private void LoadGameplayScene() =>
            _scenesLoaderService.LoadSceneNetwork(SceneName.Gameplay);

        private static void CreateSaveSelectionMenuView() =>
           MenuFactory.Create<SaveSelectionMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartGameClicked()
        {
            StartHost();
            LoadGameplayScene();
        }
    }
}