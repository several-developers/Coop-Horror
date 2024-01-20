using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Network;

namespace GameCore.Infrastructure.StateMachine
{
    public class QuitGameplayState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuitGameplayState(IGameStateMachine gameStateMachine, IHorrorStateMachine horrorStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _horrorStateMachine = horrorStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IHorrorStateMachine _horrorStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            Disconnect();
            QuitHorrorStateMachine();
            EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void Disconnect()
        {
            TheNetworkHorror network = TheNetworkHorror.Get();
            network.Disconnect();
        }

        private void QuitHorrorStateMachine() =>
            _horrorStateMachine.ChangeState<QuitState>();

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}