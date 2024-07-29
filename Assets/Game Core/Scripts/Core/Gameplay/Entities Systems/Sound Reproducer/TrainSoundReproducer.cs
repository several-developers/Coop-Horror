using GameCore.Configs.Gameplay.Train;
using GameCore.Gameplay.Entities.Train;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.SoundReproducer
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
            SoundEvent soundEvent = null;
            
            switch (sfxType)
            {
                case TrainEntity.SFXType.DoorOpen:
                    soundEvent = _trainConfig.DoorOpenSE;
                    break;
                
                case TrainEntity.SFXType.DoorClose:
                    soundEvent = _trainConfig.DoorCloseSE;
                    break;
            }

            if (soundEvent == null)
                return;

            PlaySound(soundEvent);
        }
    }
}