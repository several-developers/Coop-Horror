using System;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.SirenHead;
using GameCore.Gameplay.GameTimeManagement;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class SirenHeadSoundReproducer : SoundReproducerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SirenHeadSoundReproducer(Transform owner, SirenHeadAIConfigMeta sirenHeadAIConfig) : base(owner) =>
            _sirenHeadAIConfig = sirenHeadAIConfig;

        // FIELDS: --------------------------------------------------------------------------------

        public event Func<int> GetCurrentTimeInMunutesEvent = () => 0; 

        private readonly SirenHeadAIConfigMeta _sirenHeadAIConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(SirenHeadEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = null;

            switch (sfxType)
            {
                case SirenHeadEntity.SFXType.Footsteps:
                    soundEvent = _sirenHeadAIConfig.FootstepsSE;
                    break;

                case SirenHeadEntity.SFXType.Roar:
                    PlayRoarSound();
                    break;
            }

            if (soundEvent == null)
                return;

            PlaySound(soundEvent);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayRoarSound()
        {
            int currentTimeInMinutes = GetCurrentTimeInMunutesEvent.Invoke();
        }
    }
}