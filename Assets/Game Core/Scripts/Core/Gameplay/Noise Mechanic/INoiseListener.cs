using UnityEngine;

namespace GameCore.Gameplay.NoiseMechanic
{
    public interface INoiseListener
    {
        void DetectNoise(Vector3 noisePosition, float noiseLoudness);
    }
}