using GameCore.Enums;
using GameCore.Gameplay.Entities.MobileHeadquarters;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _mobileHeadquartersEntity.OnLoadLocationEvent += OnLoadLocation;
            _mobileHeadquartersEntity.OnLocationLeftEvent += OnLocationLeft;
        }

        public void Exit()
        {
            _mobileHeadquartersEntity.OnLoadLocationEvent -= OnLoadLocation;
            _mobileHeadquartersEntity.OnLocationLeftEvent -= OnLocationLeft;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterLoadLocationState(SceneName sceneName) =>
            _horrorStateMachine.ChangeState<LoadLocationState, SceneName>(sceneName);

        private void EnterLeaveLocationState() =>
            _horrorStateMachine.ChangeState<LeaveLocationState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLoadLocation(SceneName sceneName) => EnterLoadLocationState(sceneName);

        private void OnLocationLeft() => EnterLeaveLocationState();
    }
}