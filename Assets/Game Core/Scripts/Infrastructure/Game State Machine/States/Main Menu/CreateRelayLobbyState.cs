using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.LobbiesMenu.RelayLobby;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateRelayLobbyState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateRelayLobbyState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private RelayLobbyMenuView _relayLobbyMenu;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => CreateIPLobbyMenu();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void CreateIPLobbyMenu()
        {
            _relayLobbyMenu = MenuFactory.Create<RelayLobbyMenuView>();

            _relayLobbyMenu.OnCloseClickedEvent += OnCloseClicked;
        }

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => EnterMainMenuState();
    }
}