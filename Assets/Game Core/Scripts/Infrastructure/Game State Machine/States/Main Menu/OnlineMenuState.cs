using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.OnlineMenu;
using Unity.Netcode;

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
            _onlineMenuView.OnClientClickedEvent += OnClientClicked;
        }

        public void Exit()
        {
            _onlineMenuView.OnHostClickedEvent -= OnHostClicked;
            _onlineMenuView.OnClientClickedEvent -= OnClientClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateOnlineMenu() =>
            _onlineMenuView = MenuFactory.Create<OnlineMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHostClicked() =>
            NetworkManager.Singleton.StartHost();

        private void OnClientClicked() =>
            NetworkManager.Singleton.StartClient();
    }
}