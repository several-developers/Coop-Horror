using System;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Delivery
{
    public class DeliveryPoint : MonoBehaviour, IDeliveryPoint
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            DeliveryConfigMeta deliveryConfig = gameplayConfigsProvider.GetDeliveryConfig();
            
            _droneCart = new DroneCart(deliveryConfig, _landingPath, _takeOffPath, _deliveryCart);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _landingPath;

        [SerializeField, Required]
        private CinemachinePath _takeOffPath;

        [SerializeField, Required]
        private CinemachineDollyCart _deliveryCart;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTeleportDroneToDroneCartEvent = delegate { };

        private DroneCart _droneCart;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update() =>
            _droneCart.Tick();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public Transform GetDroneCartTransform() =>
            _deliveryCart.transform;
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        [Button(buttonSize: 30), DisableInEditorMode]
        private void LandDrone()
        {
            _droneCart.Land();
            SendTeleportDroneToDroneCart();
        }

        [Button(buttonSize: 30), DisableInEditorMode]
        private void TakeOffDrone()
        {
            _droneCart.TakeOff();
            SendTeleportDroneToDroneCart();
        }

        private async void SendTeleportDroneToDroneCart()
        {
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 4, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            OnTeleportDroneToDroneCartEvent.Invoke();
        }
    }
}