using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.LobbiesMenu.IPLobby;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateIPLobbyState : IEnterState, IExitState
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

        public void Enter()
        {
            CreateIPLobbyMenu();
        }

        public void Exit()
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------


        private void CreateIPLobbyMenu() =>
            _ipLobbyMenu = MenuFactory.Create<IPLobbyMenuView>();
    }
}