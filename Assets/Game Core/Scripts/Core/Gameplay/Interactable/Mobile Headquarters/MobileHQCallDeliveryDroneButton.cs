using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.GameManagement;
using Zenject;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class MobileHQCallDeliveryDroneButton : SimpleButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator,
            IDeliveryManagerDecorator deliveryManagerDecorator)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _deliveryManagerDecorator = deliveryManagerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;
        private IDeliveryManagerDecorator _deliveryManagerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InteractionStarted() =>
            _deliveryManagerDecorator.OnDroneStateChangedEvent += OnDroneStateChanged;

        public override void InteractionEnded() =>
            _deliveryManagerDecorator.OnDroneStateChangedEvent -= OnDroneStateChanged;

        public override bool CanInteract()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = false;

            switch (gameState)
            {
                case GameState.ReadyToLeaveTheRoad:
                    isGameStateValid = true;
                    break;
            }

            if (!isGameStateValid)
                return false;

            DroneState droneState = _deliveryManagerDecorator.GetDroneState();
            bool canCallDeliveryDrone = droneState == DroneState.WaitingForCall;

            if (!canCallDeliveryDrone)
                return false;

            return IsInteractionEnabled;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDroneStateChanged(DroneState droneState) => SendInteractionStateChangedEvent();
    }
}