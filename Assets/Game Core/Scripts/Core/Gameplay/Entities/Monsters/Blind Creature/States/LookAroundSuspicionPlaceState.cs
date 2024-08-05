namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class LookAroundSuspicionPlaceState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LookAroundSuspicionPlaceState(BlindCreatureEntity blindCreatureEntity)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly SuspicionSystem _suspicionSystem;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _suspicionSystem.OnSuspicionMeterChangedEvent += OnSuspicionMeterChanged;
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;

            TryLeaveState();
        }

        public void Exit()
        {
            _suspicionSystem.OnSuspicionMeterChangedEvent -= OnSuspicionMeterChanged;
            _suspicionSystem.OnNoiseDetectedEvent -= OnNoiseDetected;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryLeaveState()
        {
            int suspicionMeter = _suspicionSystem.GetSuspicionMeter();
            bool canLeave = suspicionMeter <= 0;

            if (!canLeave)
                return;
            
            EnterIdleState();
        }

        private void EnterIdleState() =>
            _blindCreatureEntity.EnterIdleState();

        private void EnterMoveToSuspicionPlaceState() =>
            _blindCreatureEntity.EnterMoveToSuspicionPlaceState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSuspicionMeterChanged(int suspicionMeter) => TryLeaveState();

        private void OnNoiseDetected() => EnterMoveToSuspicionPlaceState();
    }
}