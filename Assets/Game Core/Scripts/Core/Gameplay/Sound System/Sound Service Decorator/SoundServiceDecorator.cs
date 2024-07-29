using System;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.SoundSystem
{
    public class SoundServiceDecorator : ISoundServiceDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<SoundEvent, Transform> OnPlaySoundInnerEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void PlaySound(SoundEvent soundEvent, Transform owner) =>
            OnPlaySoundInnerEvent.Invoke(soundEvent, owner);
    }
}