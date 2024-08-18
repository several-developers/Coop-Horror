using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.EvilClown;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class EvilClownSoundReproducer : SoundReproducerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public EvilClownSoundReproducer(Transform owner, EvilClownAIConfigMeta evilClownAIConfig) : base(owner) =>
            _evilClownAIConfig = evilClownAIConfig;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(EvilClownEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = null;
            
            switch (sfxType)
            {
                case EvilClownEntity.SFXType.Footsteps:
                    soundEvent = _evilClownAIConfig.FootstepsSE;
                    break;
                
                case EvilClownEntity.SFXType.Roar:
                    soundEvent = _evilClownAIConfig.RoarSE;
                    break;
                
                case EvilClownEntity.SFXType.Brainwash:
                    soundEvent = _evilClownAIConfig.BrainwashSE;
                    break;
            }

            if (soundEvent == null)
                return;
            
            PlaySound(soundEvent);
        }
    }
}