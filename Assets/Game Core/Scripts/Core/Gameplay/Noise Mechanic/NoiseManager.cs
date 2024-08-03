using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.NoiseManagement
{
    public class NoiseManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            ISubscriber<NoiseDataMessage> noiseDataMessageSubscriber,
            IPublisher<NoiseDataMessage> noiseDataMessagePublisher)
        {
            _noiseDataMessageSubscriber = noiseDataMessageSubscriber;
            _noiseDataMessagePublisher = noiseDataMessagePublisher;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static ISubscriber<NoiseDataMessage> _noiseDataMessageSubscriber;
        private static IPublisher<NoiseDataMessage> _noiseDataMessagePublisher;

        private readonly Collider[] _collidersPull = new Collider[20];

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _noiseDataMessageSubscriber.Subscribe(DetectNoise);

        public override void OnDestroy()
        {
            base.OnDestroy();
            _noiseDataMessageSubscriber.Unsubscribe(DetectNoise);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void MakeNoise(Vector3 noisePosition, float noiseRange, float noiseLoudness)
        {
            NoiseDataMessage message = new()
            {
                noisePosition = noisePosition,
                noiseRange = noiseRange,
                noiseLoudness = noiseLoudness
            };
            
            _noiseDataMessagePublisher.Publish(message);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DetectNoise(NoiseDataMessage message)
        {
            Vector3 noisePosition = message.noisePosition;
            float noiseLoudness = message.noiseLoudness;
            int iterations = Physics.OverlapSphereNonAlloc(noisePosition, message.noiseRange, _collidersPull);

            for (int i = 0; i < iterations; i++)
            {
                bool isNoiseListenerFound = _collidersPull[i].TryGetComponent(out INoiseListener noiseListener);

                if (!isNoiseListenerFound)
                    continue;

                noiseListener.DetectNoise(noisePosition, noiseLoudness);
            }
        }
    }
}