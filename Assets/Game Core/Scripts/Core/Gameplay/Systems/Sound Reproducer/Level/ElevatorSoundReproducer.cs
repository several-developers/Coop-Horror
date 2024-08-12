using GameCore.Configs.Gameplay.Elevator;
using GameCore.Gameplay.Level.Elevator;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class ElevatorSoundReproducer : SoundReproducerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorSoundReproducer(Transform owner, ElevatorConfigMeta elevatorConfig) : base(owner) =>
            _elevatorConfig = elevatorConfig;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ElevatorConfigMeta _elevatorConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(ElevatorBase.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                ElevatorBase.SFXType.DoorOpening => _elevatorConfig.DoorOpeningSE,
                ElevatorBase.SFXType.DoorClosing => _elevatorConfig.DoorClosingSE,
                ElevatorBase.SFXType.FloorChange => _elevatorConfig.FloorChangeSE,
                ElevatorBase.SFXType.ButtonPush => _elevatorConfig.ButtonPushSE,
                _ => null
            };

            if (soundEvent == null)
                return;
            
            PlaySound(soundEvent);
        }
    }
}