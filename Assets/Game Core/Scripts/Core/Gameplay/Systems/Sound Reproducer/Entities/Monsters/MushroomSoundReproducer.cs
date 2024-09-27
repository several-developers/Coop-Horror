using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.Mushroom;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class MushroomSoundReproducer : SoundReproducerBase<MushroomEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MushroomSoundReproducer(
            ISoundProducer<MushroomEntity.SFXType> soundProducer,
            MushroomAIConfigMeta mushroomAIConfig
        ) : base(soundProducer)
        {
            _mushroomAIConfig = mushroomAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomAIConfigMeta _mushroomAIConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(MushroomEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                MushroomEntity.SFXType.Footsteps => _mushroomAIConfig.FootstepsSE,
                MushroomEntity.SFXType.HatExplosion => _mushroomAIConfig.HatExplosionSE,
                MushroomEntity.SFXType.HatRegeneration => _mushroomAIConfig.HatRegenerationSE,
                MushroomEntity.SFXType.Whispering => _mushroomAIConfig.WhisperingSE,
                MushroomEntity.SFXType.SitDown => _mushroomAIConfig.SitDownSE,
                MushroomEntity.SFXType.StandUp => _mushroomAIConfig.StandUpSE,
                _ => null
            };

            return soundEvent;
        }
    }
}