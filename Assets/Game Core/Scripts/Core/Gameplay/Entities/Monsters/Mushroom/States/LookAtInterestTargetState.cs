namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class LookAtInterestTargetState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LookAtInterestTargetState(MushroomEntity mushroomEntity) =>
            _mushroomEntity = mushroomEntity;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MushroomEntity _mushroomEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _mushroomEntity.DisableAgent();
            SetSneakingState(isSneaking: true);
        }

        public void Exit() => SetSneakingState(isSneaking: false);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetSneakingState(bool isSneaking) =>
            _mushroomEntity.SetSneakingState(isSneaking);
    }
}