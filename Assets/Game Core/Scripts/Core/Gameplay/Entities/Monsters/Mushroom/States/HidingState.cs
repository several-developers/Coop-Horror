using GameCore.Configs.Gameplay.Enemies;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class HidingState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public HidingState(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig();
        
            _mushroomEntity = mushroomEntity;
            _animationController = mushroomEntity.GetAnimationController();
        }
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MushroomEntity _mushroomEntity;
        private readonly AnimationController _animationController;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            _mushroomEntity.DisableAgent();
            ChangeAnimationHidingState(isHiding: true);
        }

        public void Exit()
        {
            ChangeAnimationHidingState(isHiding: false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeAnimationHidingState(bool isHiding) =>
            _animationController.ChangeHidingState(isHiding);
    }
}