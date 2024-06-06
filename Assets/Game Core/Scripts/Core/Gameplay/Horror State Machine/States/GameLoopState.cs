using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, IGameManagerDecorator gameManagerDecorator)
        {
            _horrorStateMachine = horrorStateMachine;
            _gameManagerDecorator = gameManagerDecorator;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IGameManagerDecorator _gameManagerDecorator;

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
                case GameState.RestartGame:
                    if (NetworkHorror.IsTrueServer)
                        EnterLeaveLocationServerState();
                    else
                        EnterLeaveLocationClientState();
                    break;
            }
        }

        private void EnterLeaveLocationServerState() =>
            _horrorStateMachine.ChangeState<LeaveLocationServerState>();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}