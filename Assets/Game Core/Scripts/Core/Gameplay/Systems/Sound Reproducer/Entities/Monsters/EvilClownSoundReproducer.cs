using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.EvilClown;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class EvilClownSoundReproducer : SoundReproducerBase<EvilClownEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EvilClownSoundReproducer(
            ISoundProducer<EvilClownEntity.SFXType> soundProducer,
            EvilClownAIConfigMeta evilClownAIConfig
        ) : base(soundProducer)
        {
            _evilClownAIConfig = evilClownAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly EvilClownAIConfigMeta _evilClownAIConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(EvilClownEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                EvilClownEntity.SFXType.Footsteps => _evilClownAIConfig.FootstepsSE,
                EvilClownEntity.SFXType.Roar => _evilClownAIConfig.RoarSE,
                EvilClownEntity.SFXType.Brainwash => _evilClownAIConfig.BrainwashSE,
                EvilClownEntity.SFXType.Slash => _evilClownAIConfig.SlashSE,
                _ => null
            };

            return soundEvent;
        }
    }
}