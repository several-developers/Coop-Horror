using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, IGameManagerDecorator gameManagerDecorator,
            INetworkHorror networkHorror)
        {
            _horrorStateMachine = horrorStateMachine;
            _gameManagerDecorator = gameManagerDecorator;
            _networkHorror = networkHorror;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly INetworkHorror _networkHorror;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() =>
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

        public void Exit() =>
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.ArrivedAtTheRoad:
                    if (IsServer())
                        EnterLeaveLocationServerState();
                    else
                        EnterLeaveLocationClientState();
                    break;
            }
        }

        private bool IsServer() =>
            _networkHorror.IsOwner;

        private void EnterLeaveLocationServerState() =>
            _horrorStateMachine.ChangeState<LeaveLocationServerState>();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}