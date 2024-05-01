using System;
using System.Collections;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Enums.Gameplay;
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
            _deliveryConfig = gameplayConfigsProvider.GetDeliveryConfig();
            _droneCart = new DroneCart(_deliveryConfig, deliveryPoint: this, _landingPath, _takeOffPath, _deliveryCart);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _landingPath;

        [SerializeField, Required]
        private CinemachinePath _takeOffPath;

        [SerializeField, Required]
        private CinemachineDollyCart _deliveryCart;

        [SerializeField, Required]
        private GameObject _heliport;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<DroneState> OnDroneStateChangedEvent = delegate { };
        public event Action OnTeleportDroneToDroneCartEvent = delegate { };
        public event Action OnDroneTakeOffTimerFinishedEvent = delegate { };

        private DeliveryConfigMeta _deliveryConfig;
        private DroneCart _droneCart;
        private Coroutine _takeOffTimerCO;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => HidePoint();

        private void Update() =>
            _droneCart.Tick();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ShowPoint() =>
            _heliport.SetActive(true);

        public void HidePoint() =>
            _heliport.SetActive(false);

        public void LandDrone()
        {
            _droneCart.Land();
            SendTeleportDroneToDroneCart();
        }

        public void TakeOffDrone()
        {
            _droneCart.TakeOff();
            SendTeleportDroneToDroneCart();
        }

        public void StartTakeOffTimer()
        {
            StopTakeOffTimer();
            
            _takeOffTimerCO = StartCoroutine(routine: DroneTakeOffTimeCO());
        }

        public void ResetTakeOffTimer() => StartTakeOffTimer();

        public void SendDroneStateChanged(DroneState droneState) =>
            OnDroneStateChangedEvent.Invoke(droneState);

        public Transform GetDroneCartTransform() =>
            _deliveryCart.transform;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void SendTeleportDroneToDroneCart()
        {
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 5, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            OnTeleportDroneToDroneCartEvent.Invoke();
        }

        private void StopTakeOffTimer()
        {
            if (_takeOffTimerCO == null)
                return;
            
            StopCoroutine(_takeOffTimerCO);
        }

        private IEnumerator DroneTakeOffTimeCO()
        {
            float delay = _deliveryConfig.TakeOffDelay;
            
            yield return new WaitForSeconds(delay);

            TakeOffDrone();
            OnDroneTakeOffTimerFinishedEvent.Invoke();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugLandDrone() => LandDrone();
        
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugTakeOffDrone() => TakeOffDrone();
    }
}