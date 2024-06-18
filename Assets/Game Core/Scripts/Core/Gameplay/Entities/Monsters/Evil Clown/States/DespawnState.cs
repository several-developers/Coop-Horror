namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class DespawnState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DespawnState(EvilClownEntity evilClownEntity)
        {
            _evilClownEntity = evilClownEntity;
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly WanderingTimer _wanderingTimer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            StopWanderingTimer();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopWanderingTimer() =>
            _wanderingTimer.StopTimer();
    }
}