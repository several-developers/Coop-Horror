using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.SelectLobbyMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class MainMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MainMenuState(IGameStateMachine gameStateMachine, IMenusFactory menusFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menusFactory = menusFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenusFactory _menusFactory;

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
            _selectLobbyMenu = await _menusFactory.Create<SelectLobbyMenuView>();

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