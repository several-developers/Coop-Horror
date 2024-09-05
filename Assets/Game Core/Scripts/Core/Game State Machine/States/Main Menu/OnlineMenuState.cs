using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.OnlineMenu;

namespace GameCore.StateMachine
{
    public class OnlineMenuState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OnlineMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private OnlineMenuView _onlineMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateOnlineMenu();

            _onlineMenuView.OnBackButtonClickedEvent += OnBackButtonClicked;
            _onlineMenuView.OnOpenRelayLobbyMenuClickedEvent += OnOpenRelayLobbyMenuClicked;
            _onlineMenuView.OnOpenIPLobbyMenuClickedEvent += OnOpenIPLobbyMenuClicked;
        }

        public void Exit()
        {
            if (_onlineMenuView == null)
                return;
            
            _onlineMenuView.OnBackButtonClickedEvent -= OnBackButtonClicked;
            _onlineMenuView.OnOpenRelayLobbyMenuClickedEvent -= OnOpenRelayLobbyMenuClicked;
            _onlineMenuView.OnOpenIPLobbyMenuClickedEvent -= OnOpenIPLobbyMenuClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateOnlineMenu() =>
            _onlineMenuView = MenuStaticFactory.Create<OnlineMenuView>();

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();

        private void EnterCreateLobbyState() =>
            _gameStateMachine.ChangeState<CreateLobbyState>();

        private void EnterJoinGameState() =>
            _gameStateMachine.ChangeState<JoinGameState>();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked() => EnterPrepareMainMenuState();

        private void OnOpenRelayLobbyMenuClicked()
        {
            //EnterCreateLobbyState();
        }

        private void OnOpenIPLobbyMenuClicked()
        {
            //EnterJoinGameState();
        }
    }
}