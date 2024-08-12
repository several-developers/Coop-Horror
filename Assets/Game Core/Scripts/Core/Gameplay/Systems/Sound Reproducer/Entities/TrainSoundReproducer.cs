using GameCore.Configs.Gameplay.Train;
using GameCore.Gameplay.Entities.Train;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class TrainSoundReproducer : SoundReproducerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public TrainSoundReproducer(Transform owner, TrainConfigMeta trainConfig) : base(owner) =>
            _trainConfig = trainConfig;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly TrainConfigMeta _trainConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(TrainEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = GetSoundEvent(sfxType);

            if (soundEvent == null)
                return;
            
            PlaySound(soundEvent);
        }
        
        public void StopSound(TrainEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = GetSoundEvent(sfxType);

            if (soundEvent == null)
                return;
            
            StopSound(soundEvent);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private SoundEvent GetSoundEvent(TrainEntity.SFXType sfxType)
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