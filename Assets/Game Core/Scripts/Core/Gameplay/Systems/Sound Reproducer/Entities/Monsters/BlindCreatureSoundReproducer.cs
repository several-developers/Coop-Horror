using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.BlindCreature;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class BlindCreatureSoundReproducer : SoundReproducerBase<BlindCreatureEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public BlindCreatureSoundReproducer(
            ISoundProducer<BlindCreatureEntity.SFXType> soundProducer,
            BlindCreatureAIConfigMeta blindCreatureAIConfig
        ) : base(soundProducer)
        {
            _blindCreatureAIConfig = blindCreatureAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(BlindCreatureEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                BlindCreatureEntity.SFXType.Whispering => _blindCreatureAIConfig.WhisperingSE,
                BlindCreatureEntity.SFXType.Swing => _blindCreatureAIConfig.SwingSE,
                BlindCreatureEntity.SFXType.Slash => _blindCreatureAIConfig.SlashSE,
                BlindCreatureEntity.SFXType.Whispers => _blindCreatureAIConfig.WhispersSE,
                BlindCreatureEntity.SFXType.BirdChirp => _blindCreatureAIConfig.BirdChirpSE,
                BlindCreatureEntity.SFXType.BirdScream => _blindCreatureAIConfig.BirdScreamSE,
                _ => null
            };

            return soundEvent;
        }
    }
}