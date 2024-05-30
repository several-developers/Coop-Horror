namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class ScreamState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ScreamState(BeetleEntity beetleEntity) =>
            _beetleEntity = beetleEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => EnterChaseState();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void EnterChaseState() =>
            _beetleEntity.EnterChaseState();
    }
}