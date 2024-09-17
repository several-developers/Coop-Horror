using System;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Interactable.Train
{
    public class TrainSeat : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private float _spawnPositionY = 1.2f;

        [Title(Constants.References)]
        [SerializeField, Required]
        private BoxCollider _boxCollider;

        [SerializeField, Required]
        private Transform _teleportPoint;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Transform TeleportPoint => _teleportPoint;
        public int SeatIndex { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        private const Ease PlayerAnimationEase = Ease.InQuad;
        private const float PlayerRotationDuration = 0.35f;

        public event Action OnInteractionStateChangedEvent;
        public event Action<int> OnTakeSeatEvent = delegate { };
        public event Action<int> OnLeftSeatEvent = delegate { };
        public event Func<int, bool> IsSeatBusyEvent = _ => true;
        public event Func<bool> ShouldRemovePlayerParentEvent = () => false;

        private Tweener _playerPositionTN;
        private Tweener _playerRotationTN;

        private PlayerEntity _lastPlayerEntity;
        private bool _isInteractionEnabled = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted(IEntity entity = null)
        {
        }

        public void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
        {
            if (entity is not PlayerEntity)
                return;

            PlayerEntity playerEntity = GetLocalPlayer();
            bool hasParent = playerEntity.transform.parent != null;

            if (hasParent)
            {
                InteractLogic();
                return;
            }

            playerEntity.OnParentChangedEvent += OnPlayerParentChanged;
            playerEntity.SetTrainAsParent();
        }

        public void ToggleInteract(bool canInteract)
        {
            _isInteractionEnabled = canInteract;
            SendInteractionStateChangedEvent();
        }

        public void ToggleCollider(bool isEnabled) =>
            _boxCollider.enabled = isEnabled;

        public void SetSeatIndex(int seatIndex) =>
            SeatIndex = seatIndex;

        public InteractionType GetInteractionType() =>
            InteractionType.MobileHQSeat;

        public bool CanInteract() =>
            _isInteractionEnabled && !IsSeatBusyEvent.Invoke(SeatIndex);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void InteractLogic()
        {
            PlayerEntity playerEntity = GetLocalPlayer();
            Transform playerTransform = playerEntity.transform;
            Vector3 endPosition = _teleportPoint.localPosition + transform.localPosition;
            endPosition.y = _spawnPositionY;

            _playerPositionTN.Kill();
            _playerRotationTN.Kill();

            _playerPositionTN = playerTransform
                .DOLocalMove(endPosition, PlayerRotationDuration)
                .SetEase(PlayerAnimationEase);

            _playerRotationTN = playerTransform
                .DOLocalRotate(transform.localRotation.eulerAngles, PlayerRotationDuration)
                .SetEase(PlayerAnimationEase);

            playerEntity.EnterSittingState();
            playerEntity.OnLeftMobileHQSeat += OnLeftMobileHQSeat;

            OnTakeSeatEvent.Invoke(SeatIndex);
        }

        private void SendInteractionStateChangedEvent() =>
            OnInteractionStateChangedEvent?.Invoke();

        private static PlayerEntity GetLocalPlayer() =>
            PlayerEntity.GetLocalPlayer();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerParentChanged(bool hasParent)
        {
            PlayerEntity playerEntity = GetLocalPlayer();
            playerEntity.OnParentChangedEvent -= OnPlayerParentChanged;
            InteractLogic();
        }

        private void OnLeftMobileHQSeat()
        {
            _playerPositionTN.Kill();
            _playerRotationTN.Kill();

            PlayerEntity playerEntity = GetLocalPlayer();
            bool shouldRemovePlayerParent = ShouldRemovePlayerParentEvent.Invoke();
            
            if (shouldRemovePlayerParent)
                playerEntity.RemoveParent();

            playerEntity.OnLeftMobileHQSeat -= OnLeftMobileHQSeat;

            OnLeftSeatEvent.Invoke(SeatIndex);
        }
    }
}