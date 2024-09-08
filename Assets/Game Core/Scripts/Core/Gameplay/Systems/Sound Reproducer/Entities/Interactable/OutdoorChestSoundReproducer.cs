using GameCore.Configs.Gameplay.Entities;
using GameCore.Gameplay.Entities.Interactable.OutdoorChest;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class OutdoorChestSoundReproducer : SoundReproducerBase<OutdoorChestEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public OutdoorChestSoundReproducer(
            ISoundProducer<OutdoorChestEntity.SFXType> soundProducer,
            OutdoorChestConfigMeta outdoorChestConfig
        ) : base(soundProducer)
        {
            _outdoorChestConfig = outdoorChestConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly OutdoorChestConfigMeta _outdoorChestConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override SoundEvent GetSoundEvent(OutdoorChestEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                OutdoorChestEntity.SFXType.Open => _outdoorChestConfig.OpenSE,
                _ => null
            };

            return soundEvent;
        }
    }
}