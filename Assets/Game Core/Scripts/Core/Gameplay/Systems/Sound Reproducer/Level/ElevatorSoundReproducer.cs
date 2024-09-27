using GameCore.Infrastructure.Configs.Gameplay.Elevator;
using GameCore.Gameplay.Entities.Level.Elevator;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class ElevatorSoundReproducer : SoundReproducerBase<ElevatorEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorSoundReproducer(
            ISoundProducer<ElevatorEntity.SFXType> soundProducer,
            ElevatorConfigMeta elevatorConfig
            ) : base(soundProducer)
        {
            _elevatorConfig = elevatorConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ElevatorConfigMeta _elevatorConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(ElevatorEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                ElevatorEntity.SFXType.DoorOpening => _elevatorConfig.DoorOpeningSE,
                ElevatorEntity.SFXType.DoorClosing => _elevatorConfig.DoorClosingSE,
                ElevatorEntity.SFXType.FloorChange => _elevatorConfig.FloorChangeSE,
                ElevatorEntity.SFXType.ButtonPush => _elevatorConfig.ButtonPushSE,
                _ => null
            };

            return soundEvent;
        }
    }
}