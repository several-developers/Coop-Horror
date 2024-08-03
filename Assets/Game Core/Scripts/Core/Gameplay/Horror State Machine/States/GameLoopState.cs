using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Network;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, ITrainEntity trainEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _trainEntity = trainEntity;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ITrainEntity _trainEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() =>
            _trainEntity.OnLeaveLocationEvent += LeaveLocation;

        public void Exit() =>
            _trainEntity.OnLeaveLocationEvent -= LeaveLocation;

        // PRIVATE METHODS: -----------------------------------------------------------------------

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
    }
}