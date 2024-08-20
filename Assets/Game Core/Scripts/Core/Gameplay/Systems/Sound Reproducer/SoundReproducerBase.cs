using System;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public abstract class SoundReproducerBase<TSFXType> where TSFXType : Enum
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected SoundReproducerBase(ISoundProducer<TSFXType> soundProducer)
        {
            _owner = soundProducer.GetTransform();

            soundProducer.OnPlaySoundEvent += PlaySound;
            soundProducer.OnStopSoundEvent += StopSound;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Transform _owner;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(TSFXType sfxType)
        {
            SoundEvent soundEvent = GetSoundEvent(sfxType);

            if (soundEvent == null)
                return;
            
            soundEvent.Play(_owner);
        }
        
        public void StopSound(TSFXType sfxType)
        {
            SoundEvent soundEvent = GetSoundEvent(sfxType);

            if (soundEvent == null)
                return;
            
            soundEvent.Stop(_owner);
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected abstract SoundEvent GetSoundEvent(TSFXType sfxType);

        // На память.
        // private static TSFXType ConvertToSFXType(int sfxTypeIndex) =>
        //     (TSFXType)Enum.ToObject(typeof(TSFXType), sfxTypeIndex);
    }
}