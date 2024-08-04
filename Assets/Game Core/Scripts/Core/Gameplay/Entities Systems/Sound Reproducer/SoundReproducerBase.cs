using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.SoundReproducer
{
    public abstract class SoundReproducerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected SoundReproducerBase(Transform owner) =>
            _owner = owner;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Transform _owner;
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void PlaySound(SoundEvent soundEvent) =>
            soundEvent.Play(_owner);

        protected void StopSound(SoundEvent soundEvent) =>
            soundEvent.Stop(_owner);
    }
}