using GameCore.Configs.Gameplay.Elevator;
using GameCore.Gameplay.Level.Elevator;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class ElevatorSoundReproducer : SoundReproducerBase<ElevatorBase.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorSoundReproducer(
            ISoundProducer<ElevatorBase.SFXType> soundProducer,
            ElevatorConfigMeta elevatorConfig
            ) : base(soundProducer)
        {
            _elevatorConfig = elevatorConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ElevatorConfigMeta _elevatorConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(ElevatorBase.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                ElevatorBase.SFXType.DoorOpening => _elevatorConfig.DoorOpeningSE,
                ElevatorBase.SFXType.DoorClosing => _elevatorConfig.DoorClosingSE,
                ElevatorBase.SFXType.FloorChange => _elevatorConfig.FloorChangeSE,
                ElevatorBase.SFXType.ButtonPush => _elevatorConfig.ButtonPushSE,
                _ => null
            };

            return soundEvent;
        }
    }
}