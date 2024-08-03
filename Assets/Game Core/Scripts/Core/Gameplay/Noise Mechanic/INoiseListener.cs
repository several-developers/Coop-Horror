using UnityEngine;

namespace GameCore.Gameplay.NoiseManagement
{
    public interface INoiseListener
    {
        void DetectNoise(Vector3 noisePosition, float noiseLoudness);
    }
}