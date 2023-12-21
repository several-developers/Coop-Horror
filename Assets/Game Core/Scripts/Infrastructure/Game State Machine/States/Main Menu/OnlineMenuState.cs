using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.OnlineMenu;

namespace GameCore.Infrastructure.StateMachine
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

            _onlineMenuView.OnHostClickedEvent += OnHostClicked;
            _onlineMenuView.OnJoinClickedEvent += OnJoinClicked;
        }

        public void Exit()
        {
            _onlineMenuView.OnHostClickedEvent -= OnHostClicked;
            _onlineMenuView.OnJoinClickedEvent -= OnJoinClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateOnlineMenu() =>
            _onlineMenuView = MenuFactory.Create<OnlineMenuView>();

        private void EnterCreateLobbyState() =>
            _gameStateMachine.ChangeState<CreateLobbyState>();

        private void EnterJoinGameState() =>
            _gameStateMachine.ChangeState<JoinGameState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHostClicked() => EnterCreateLobbyState();

        private void OnJoinClicked() => EnterJoinGameState();
    }
}