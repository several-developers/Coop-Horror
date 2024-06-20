namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class IdleState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _clownUtilities = goodClownEntity.GetClownUtilities();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownUtilities _clownUtilities;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter() => SetIdleAnimation();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetIdleAnimation() =>
            _clownUtilities.SetIdleAnimation();
    }
}