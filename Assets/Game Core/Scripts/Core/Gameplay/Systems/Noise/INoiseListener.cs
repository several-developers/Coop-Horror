using UnityEngine;

namespace GameCore.Gameplay.Systems.Noise
{
    public interface INoiseListener
    {
        void DetectNoise(Vector3 noisePosition, float noiseLoudness);
    }
}