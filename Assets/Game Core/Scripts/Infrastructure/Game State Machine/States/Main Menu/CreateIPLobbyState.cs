using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.LobbiesMenu.IPLobby;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateIPLobbyState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateIPLobbyState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private IPLobbyMenuView _ipLobbyMenu;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => CreateIPLobbyMenu();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void CreateIPLobbyMenu()
        {
            _ipLobbyMenu = MenuFactory.Create<IPLobbyMenuView>();

            _ipLobbyMenu.OnCloseClickedEvent += OnCloseClicked;
        }

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => EnterMainMenuState();
    }
}