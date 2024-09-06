using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.SelectLobbyMenu;

namespace GameCore.StateMachine
{
    public class MainMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MainMenuState(IGameStateMachine gameStateMachine, IMenuFactory menuFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;

        private SelectLobbyMenuView _selectLobbyMenu;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            //EnterCreateRelayLobbyState();
            CreateSelectLobbyMenu();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void CreateSelectLobbyMenu()
        {
            _selectLobbyMenu = await _menuFactory.Create<SelectLobbyMenuView>();

            _selectLobbyMenu.OnStartWithLobbyClickedEvent += OnStartWithLobbyClicked;
            _selectLobbyMenu.OnStartWithDirectIPClickedEvent += OnStartWithDirectIPClicked;
        }

        private void EnterCreateIPLobbyState() =>
            _gameStateMachine.ChangeState<CreateIPLobbyState>();

        private void EnterCreateRelayLobbyState() =>
            _gameStateMachine.ChangeState<CreateRelayLobbyState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartWithLobbyClicked() => EnterCreateRelayLobbyState();

        private void OnStartWithDirectIPClicked() => EnterCreateIPLobbyState();
    }
}