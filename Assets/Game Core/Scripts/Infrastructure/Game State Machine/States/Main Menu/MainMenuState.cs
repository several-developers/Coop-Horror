using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.SelectLobbyMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class MainMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MainMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private SelectLobbyMenuView _selectLobbyMenu;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateSelectLobbyMenu();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateSelectLobbyMenu()
        {
            _selectLobbyMenu = MenuFactory.Create<SelectLobbyMenuView>();

            _selectLobbyMenu.OnStartWithLobbyClickedEvent += OnStartWithLobbyClicked;
            _selectLobbyMenu.OnStartWithDirectIPClickedEvent += OnStartWithDirectIPClicked;
        }

        private void EnterCreateIPLobbyState() =>
            _gameStateMachine.ChangeState<CreateIPLobbyState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartWithLobbyClicked()
        {
            
        }
        
        private void OnStartWithDirectIPClicked() => EnterCreateIPLobbyState();
    }
}