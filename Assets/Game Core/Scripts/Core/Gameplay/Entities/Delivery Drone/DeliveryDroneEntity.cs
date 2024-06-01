using DG.Tweening;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.DeliveryDrone
{
    public class DeliveryDroneEntity : NetcodeBehaviour, IEntity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider, IDeliveryPoint deliveryPoint)
        {
            _deliveryPoint = deliveryPoint;
            
            DeliveryConfigMeta deliveryConfig = gameplayConfigsProvider.GetDeliveryConfig();
            Transform droneCartTransform = deliveryPoint.GetDroneCartTransform();

            _droneMovement = new DroneMovement(deliveryConfig, droneCartTransform, droneTransform: transform);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _redLightMaxIntensity = 3f;
        
        [SerializeField, Min(0)]
        private float _redLightPulseDuration = 0.5f;

        [SerializeField]
        private Ease _redLightPulseEase = Ease.InOutQuad;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Light _redLight;

        // FIELDS: --------------------------------------------------------------------------------

        private IDeliveryPoint _deliveryPoint;
        private DroneMovement _droneMovement;
        private Tweener _redLightPulseTN;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => PlayLightPulseAnimation(fadeIn: false);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Teleport(Vector3 position, Quaternion rotation)
        {
        }
        
        public Transform GetTransform() => transform;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            _droneMovement.TeleportDroneToDroneCart();
            
            _deliveryPoint.OnTeleportDroneToDroneCartEvent += OnTeleportDroneToDroneCart;
        }

        protected override void TickServerOnly() =>
            _droneMovement.Tick();

        protected override void DespawnServerOnly() =>
            _deliveryPoint.OnTeleportDroneToDroneCartEvent -= OnTeleportDroneToDroneCart;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayLightPulseAnimation(bool fadeIn)
        {
            float endValue = fadeIn ? _redLightMaxIntensity : 0f;
            float duration = _redLightPulseDuration;
            Ease ease = _redLightPulseEase;
            
            _redLightPulseTN.Kill();

            _redLightPulseTN = _redLight
                .DOIntensity(endValue, duration)
                .SetEase(ease)
                .SetLink(gameObject)
                .OnComplete(() => PlayLightPulseAnimation(!fadeIn));
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTeleportDroneToDroneCart() =>
            _droneMovement.TeleportDroneToDroneCart();
    }
}