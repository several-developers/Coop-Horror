using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.ConnectionManagement;

namespace GameCore.Infrastructure.StateMachine
{
    public class QuitGameplaySceneState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuitGameplaySceneState(IGameStateMachine gameStateMachine, IHorrorStateMachine horrorStateMachine,
            ConnectionManager connectionManager)
        {
            _gameStateMachine = gameStateMachine;
            _horrorStateMachine = horrorStateMachine;
            _connectionManager = connectionManager;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ConnectionManager _connectionManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            Disconnect();
            QuitHorrorStateMachine();
            EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Disconnect()
        {
            //TheNetworkHorror network = TheNetworkHorror.Get();
            //network.Disconnect();
            
            _connectionManager.RequestShutdown();
        }

        private void QuitHorrorStateMachine() =>
            _horrorStateMachine.ChangeState<QuitState>();

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}