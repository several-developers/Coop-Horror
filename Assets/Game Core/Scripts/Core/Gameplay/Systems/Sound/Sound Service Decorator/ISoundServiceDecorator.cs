using System;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Sound
{
    public interface ISoundServiceDecorator
    {
        event Action<SoundEvent, Transform> OnPlaySoundInnerEvent; 
        void PlaySound(SoundEvent soundEvent, Transform owner);
    }
}