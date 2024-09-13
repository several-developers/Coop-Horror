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
            _commonConfig = mushroomAIConfig.GetCommonConfig();
            _whisperingSystem = mushroomEntity.GetWhisperingSystem();
            _animationController = mushroomEntity.GetAnimationController();
        }
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.CommonConfig _commonConfig;
        private readonly WhisperingSystem _whisperingSystem;
        private readonly AnimationController _animationController;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            _animationController.OnHideCompletedEvent += OnHideCompleted;

            _mushroomEntity.DisableAgent();
            ChangeAnimationHidingState(isHiding: true);
            PauseWhisperingSystem();
            PlaySitDownSound();
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
        
        private void PauseWhisperingSystem() =>
            _whisperingSystem.Pause();

        private void PlaySitDownSound()
        {
            float delay = _commonConfig.SitDownSoundDelay;
            _mushroomEntity.PlaySound(MushroomEntity.SFXType.SitDown, delay).Forget();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHideCompleted() => ChangeHatTriggerState(isHiding: true);
    }
}