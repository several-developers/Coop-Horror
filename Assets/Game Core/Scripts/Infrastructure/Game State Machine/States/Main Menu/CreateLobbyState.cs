using GameCore.Enums;
using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.MainMenu.CreateLobbyMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

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
        private SaveSelectionMenuView _saveSelectionMenuView;

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

        private void CreateSaveSelectionMenuView() => 
            _saveSelectionMenuView = MenuFactory.Create<SaveSelectionMenuView>();

        private static void StartHost()
        {
            TheNetworkHorror network = TheNetworkHorror.Get();
            network.StartHost();
        }

        private void LoadGameplayScene() =>
            _scenesLoaderService.LoadSceneNetwork(SceneName.Gameplay);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartGameClicked()
        {
            StartHost();
            LoadGameplayScene();
            _saveSelectionMenuView.Hide();
        }
    }
}