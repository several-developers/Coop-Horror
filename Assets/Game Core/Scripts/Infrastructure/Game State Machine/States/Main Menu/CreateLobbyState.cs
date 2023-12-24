using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Network;
using GameCore.UI.MainMenu.CreateLobbyMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateLobbyState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateLobbyState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private CreateLobbyMenuView _createLobbyMenuView;
        private SaveSelectionMenuView _saveSelectionMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateCreateLobbyMenu();
            CreateSaveSelectionMenuView();

            _createLobbyMenuView.OnBackButtonClickedEvent += OnBackButtonClicked;
            _createLobbyMenuView.OnStartGameClickedEvent += OnStartGameClicked;
        }

        public void Exit()
        {
            _createLobbyMenuView.OnBackButtonClickedEvent -= OnBackButtonClicked;
            _createLobbyMenuView.OnStartGameClickedEvent -= OnStartGameClicked;
        }

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

        private void HideSaveSelectionMenu() =>
            _saveSelectionMenuView.Hide();

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();
        
        private void EnterLoadGameplayState() =>
            _gameStateMachine.ChangeState<LoadGameplayState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            HideSaveSelectionMenu();
            EnterOnlineMenuState();
        }
        
        private void OnStartGameClicked()
        {
            HideSaveSelectionMenu();
            StartHost();
            EnterLoadGameplayState();
        }
    }
}