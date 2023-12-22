using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Network;
using GameCore.UI.MainMenu.ConnectingMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class JoinGameState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JoinGameState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            // ConnectingMenuView connectingMenuView = CreateConnectingMenu();
            // connectingMenuView.OnQuitClickedEvent += OnQuitConnecting;
            //
            // JoinGame();

            StartClient();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void StartClient()
        {
            TheNetworkHorror network = TheNetworkHorror.Get();
            network.StartClient();
        }

        private static ConnectingMenuView CreateConnectingMenu() =>
            MenuFactory.Create<ConnectingMenuView>();

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnQuitConnecting() => EnterOnlineMenuState();
    }
}