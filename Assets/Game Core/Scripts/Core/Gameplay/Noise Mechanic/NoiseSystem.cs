using System;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.NoiseMechanic
{
    public class NoiseSystem : NetcodeBehaviour
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

        public static event Action<NoiseDataMessage> OnNoiseDetectedEvent = delegate { };
        
        private static ISubscriber<NoiseDataMessage> _noiseDataMessageSubscriber;
        private static IPublisher<NoiseDataMessage> _noiseDataMessagePublisher;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _noiseDataMessageSubscriber.Subscribe(OnNoiseDataMessageReceived);

        public override void OnDestroy()
        {
            base.OnDestroy();
            _noiseDataMessageSubscriber.Unsubscribe(OnNoiseDataMessageReceived);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void MakeNoise(Vector3 noisePosition, float noiseLoudness)
        {
            NoiseDataMessage message = new()
            {
                noisePosition = noisePosition,
                noiseLoudness = noiseLoudness
            };
            
            _noiseDataMessagePublisher.Publish(message);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private static void OnNoiseDataMessageReceived(NoiseDataMessage message) =>
            OnNoiseDetectedEvent.Invoke(message);
    }
}