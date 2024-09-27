using GameCore.Infrastructure.Configs.Gameplay.Enemies;

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
            PlayStandUpSound();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeAnimationHidingState(bool isHiding) =>
            _mushroomEntity.SetHidingState(isHiding);

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
            PlaySound(MushroomEntity.SFXType.SitDown, delay);
        }
        
        private void PlayStandUpSound() =>
            PlaySound(MushroomEntity.SFXType.StandUp);

        private void PlaySound(MushroomEntity.SFXType sfxType, float delay = 0f) =>
            _mushroomEntity.PlaySound(sfxType, delay).Forget();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHideCompleted() => ChangeHatTriggerState(isHiding: true);
    }
}