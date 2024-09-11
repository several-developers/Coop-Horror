namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class RunawayState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public RunawayState(MushroomEntity mushroomEntity)
        {
            _mushroomEntity = mushroomEntity;
            _animationController = mushroomEntity.GetAnimationController();
        }
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MushroomEntity _mushroomEntity;
        private readonly AnimationController _animationController;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            
        }

        public void Exit()
        {
            
        }
    }
}