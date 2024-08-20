using System;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.SirenHead;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class SirenHeadSoundReproducer : SoundReproducerBase<SirenHeadEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SirenHeadSoundReproducer(
            ISoundProducer<SirenHeadEntity.SFXType> soundProducer,
            SirenHeadAIConfigMeta sirenHeadAIConfig
        ) : base(soundProducer)
        {
            _sirenHeadAIConfig = sirenHeadAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Func<int> GetCurrentTimeInMinutesEvent = () => 0;

        private readonly SirenHeadAIConfigMeta _sirenHeadAIConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(SirenHeadEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                SirenHeadEntity.SFXType.Footsteps => _sirenHeadAIConfig.FootstepsSE,
                SirenHeadEntity.SFXType.Roar => GetRoarSound(),
                _ => null
            };

            return soundEvent;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private SoundEvent GetRoarSound()
        {
            int currentTimeInMinutes = GetCurrentTimeInMinutesEvent.Invoke();
            _sirenHeadAIConfig.TryGetRoarSE(currentTimeInMinutes, out SoundEvent soundEvent);
            return soundEvent;
        }
    }
}