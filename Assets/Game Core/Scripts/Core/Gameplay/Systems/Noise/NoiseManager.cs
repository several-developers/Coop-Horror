using GameCore.Configs.Global.Game;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Systems.Noise
{
    public class NoiseManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IConfigsProvider configsProvider,
            ISubscriber<NoiseDataMessage> noiseDataMessageSubscriber,
            IPublisher<NoiseDataMessage> noiseDataMessagePublisher)
        {
            _gameConfig = configsProvider.GetConfig<GameConfigMeta>();
            _noiseDataMessageSubscriber = noiseDataMessageSubscriber;
            _noiseDataMessagePublisher = noiseDataMessagePublisher;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Required]
        private NoiseDrawer _noiseDrawer;

        // FIELDS: --------------------------------------------------------------------------------

        private static ISubscriber<NoiseDataMessage> _noiseDataMessageSubscriber;
        private static IPublisher<NoiseDataMessage> _noiseDataMessagePublisher;

        private readonly Collider[] _collidersPull = new Collider[32];

        private GameConfigMeta _gameConfig;

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
            if (!NetworkHorror.IsTrueServer)
                return;
            
            Vector3 noisePosition = message.noisePosition;
            float noiseLoudness = message.noiseLoudness;
            float noiseRange = message.noiseRange;

            LayerMask noiseLayers = _gameConfig.NoiseLayers;
            int hits = Physics.OverlapSphereNonAlloc(noisePosition, noiseRange, _collidersPull, noiseLayers);
            
            for (int i = 0; i < hits; i++)
            {
                bool isNoiseListenerFound = _collidersPull[i].TryGetComponent(out INoiseListener noiseListener);

                if (!isNoiseListenerFound)
                    continue;

                noiseListener.DetectNoise(noisePosition, noiseLoudness);
            }

#if UNITY_EDITOR
            _noiseDrawer.Draw(noisePosition, noiseRange);
#endif
        }
    }
}