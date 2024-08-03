using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.PubSub.Messages
{
    public struct NoiseDataMessage : INetworkSerializeByMemcpy
    {
        // FIELDS: --------------------------------------------------------------------------------

        public Vector3 noisePosition;
        public float noiseLoudness;
        // Tag
    }
}