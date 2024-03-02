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

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        //private void CreateCreateLobbyMenu() =>
            //_createLobbyMenuView = MenuFactory.Create<CreateLobbyMenuView>();
    }
}