using GameCore.Infrastructure.Configs.Global.Game;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub.Messages;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Systems.Noise
{
    public class NoiseManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IConfigsProvider configsProvider)
        {
            _gameConfig = configsProvider.GetConfig<GameConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Required]
        private NoiseDrawer _noiseDrawer;

        // FIELDS: --------------------------------------------------------------------------------

        private static NoiseManager _noiseManager;

        private readonly Collider[] _collidersPull = new Collider[32];

        private GameConfigMeta _gameConfig;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _noiseManager = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void MakeNoise(Vector3 noisePosition, float noiseRange, float noiseLoudness)
        {
            bool isServer = _noiseManager.IsServerOnly;

            NoiseDataMessage message = new()
            {
                noisePosition = noisePosition,
                noiseRange = noiseRange,
                noiseLoudness = noiseLoudness
            };

            if (isServer)
                _noiseManager.DetectNoise(message);
            else
                _noiseManager.MakeNoiseServerRpc(message);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DetectNoise(NoiseDataMessage message)
        {
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

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void MakeNoiseServerRpc(NoiseDataMessage message) => DetectNoise(message);
    }
}