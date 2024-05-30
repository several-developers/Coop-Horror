namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class DeathState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(BeetleEntity beetleEntity) =>
            _beetleEntity = beetleEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => ResetAggressionSystem();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ResetAggressionSystem()
        {
            AggressionSystem aggressionSystem = _beetleEntity.GetAggressionSystem();
            aggressionSystem.Reset();
        }
    }
}