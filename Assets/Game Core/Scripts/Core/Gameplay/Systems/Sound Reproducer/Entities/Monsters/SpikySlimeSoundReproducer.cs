using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.SpikySlime;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class SpikySlimeSoundReproducer : SoundReproducerBase<SpikySlimeEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SpikySlimeSoundReproducer(
            ISoundProducer<SpikySlimeEntity.SFXType> soundProducer,
            SpikySlimeAIConfigMeta spikySlimeAIConfig
        ) : base(soundProducer)
        {
            _spikySlimeAIConfig = spikySlimeAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly SpikySlimeAIConfigMeta _spikySlimeAIConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(SpikySlimeEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                SpikySlimeEntity.SFXType.CalmMovement => _spikySlimeAIConfig.CalmMovementSE,
                SpikySlimeEntity.SFXType.AngryMovement => _spikySlimeAIConfig.AngryMovementSE,
                SpikySlimeEntity.SFXType.Calming => _spikySlimeAIConfig.CalmingSE,
                SpikySlimeEntity.SFXType.Angry => _spikySlimeAIConfig.AngrySE,
                SpikySlimeEntity.SFXType.Attack => _spikySlimeAIConfig.AttackSE,
                SpikySlimeEntity.SFXType.Stab => _spikySlimeAIConfig.StabSE,
                _ => null
            };

            return soundEvent;
        }
    }
}