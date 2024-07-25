using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(
            IHorrorStateMachine horrorStateMachine,
            IGameManagerDecorator gameManagerDecorator,
            ITrainEntity trainEntity
        )
        {
            _horrorStateMachine = horrorStateMachine;
            _gameManagerDecorator = gameManagerDecorator;
            _trainEntity = trainEntity;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly ITrainEntity _trainEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            _trainEntity.OnLeaveLocationEvent += LeaveLocation;
        }

        public void Exit()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
            
            _trainEntity.OnLeaveLocationEvent -= LeaveLocation;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            if (gameState != GameState.RestartGame)
                return;
            
            LeaveLocation();
        }

        private void LeaveLocation()
        {
            if (NetworkHorror.IsTrueServer)
                EnterLeaveLocationServerState();
            else
                EnterLeaveLocationClientState();
        }

        private void EnterLeaveLocationServerState() =>
            _horrorStateMachine.ChangeState<LeaveLocationServerState>();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}