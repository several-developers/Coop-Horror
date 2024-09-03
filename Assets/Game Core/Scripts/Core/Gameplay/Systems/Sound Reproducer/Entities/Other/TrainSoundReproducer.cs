using GameCore.Configs.Gameplay.Train;
using GameCore.Gameplay.Entities.Train;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class TrainSoundReproducer : SoundReproducerBase<TrainEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TrainSoundReproducer(ISoundProducer<TrainEntity.SFXType> soundProducer, TrainConfigMeta trainConfig)
            : base(soundProducer)
        {
            _trainConfig = trainConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly TrainConfigMeta _trainConfig;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(TrainEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                TrainEntity.SFXType.DoorOpen => _trainConfig.DoorOpenSE,
                TrainEntity.SFXType.DoorClose => _trainConfig.DoorCloseSE,
                TrainEntity.SFXType.Departure => _trainConfig.DepartureSE,
                TrainEntity.SFXType.Arrival => _trainConfig.ArrivalSE,
                TrainEntity.SFXType.MovementLoop => _trainConfig.MovementLoopSE,
                _ => null
            };

            return soundEvent;
        }
    }
}