namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class AttackSuspicionPlaceState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackSuspicionPlaceState(BlindCreatureEntity blindCreatureEntity)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _combatSystem = blindCreatureEntity.GetCombatSystem();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly CombatSystem _combatSystem;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;

            TryAttack();
        }

        public void Exit() =>
            _suspicionSystem.OnNoiseDetectedEvent -= OnNoiseDetected;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryAttack()
        {
            bool isAttackOnCooldown = _combatSystem.IsAttackOnCooldown();
            
            if (!isAttackOnCooldown)
                _combatSystem.TryStartAttackAnimation();
            
            EnterLookAroundSuspicionPlaceState();
        }

        private void EnterLookAroundSuspicionPlaceState() =>
            _blindCreatureEntity.EnterLookAroundSuspicionPlaceState();

        private void EnterMoveToSuspicionPlaceState() =>
            _blindCreatureEntity.EnterMoveToSuspicionPlaceState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNoiseDetected()
        {
            bool isTargetAtRange = _combatSystem.IsTargetAtRange();
            bool isAttackOnCooldown = _combatSystem.IsAttackOnCooldown();
            
            if (isTargetAtRange && !isAttackOnCooldown)
                TryAttack();
            else
                EnterMoveToSuspicionPlaceState();
        }
    }
}