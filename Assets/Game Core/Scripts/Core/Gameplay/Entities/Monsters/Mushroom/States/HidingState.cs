namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class HidingState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public HidingState(MushroomEntity mushroomEntity)
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
            _animationController.OnHideCompletedEvent += OnHideCompleted;

            _mushroomEntity.DisableAgent();
            ChangeAnimationHidingState(isHiding: true);
        }

        public void Exit()
        {
            _animationController.OnHideCompletedEvent -= OnHideCompleted;
            
            ChangeAnimationHidingState(isHiding: false);
            ChangeHatTriggerState(isHiding: false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeAnimationHidingState(bool isHiding) =>
            _animationController.SetHidingState(isHiding);

        private void ChangeHatTriggerState(bool isHiding)
        {
            MushroomReferences references = _mushroomEntity.GetReferences();
            references.PlayerTrigger.ChangeTriggerState(isHiding);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHideCompleted() => ChangeHatTriggerState(isHiding: true);
    }
}