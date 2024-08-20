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
                BlindCreatureEntity.SFXType.BirdTweet => _blindCreatureAIConfig.BirdTweetSE,
                BlindCreatureEntity.SFXType.BirdScream => _blindCreatureAIConfig.BirdScreamSE,
                _ => null
            };

            return soundEvent;
        }
    }
}