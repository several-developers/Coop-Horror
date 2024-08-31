using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Interactable.Outdoor_Chest
{
    public class OutdoorChestEntity : NetcodeBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsFactory itemsFactory)
        {
            _itemsFactory = itemsFactory;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent = delegate { };

        private readonly NetworkVariable<bool> _isOpen = new(writePerm: Constants.OwnerPermission);

        private IItemsFactory _itemsFactory;
        
        private bool _canInteract = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void InteractionStarted(IEntity entity = null)
        {
        }

        public void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
        {
            if (IsOwner)
                OpenChestLocal();
            else
                OpenChestServerRpc();
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            OnInteractionStateChangedEvent.Invoke();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.OutdoorChest;

        public bool CanInteract() =>
            _canInteract && !_isOpen.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll() =>
            _isOpen.OnValueChanged += OnChestOpenStateChanged;

        protected override void DespawnAll() =>
            _isOpen.OnValueChanged -= OnChestOpenStateChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OpenChestLocal()
        {
            _isOpen.Value = true;
            _animator.SetBool(id: AnimatorHashes.IsOpen, value: true);
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void OpenChestServerRpc() => OpenChestLocal();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnChestOpenStateChanged(bool previousValue, bool newValue)
        {
            bool isOpen = newValue;

            if (!isOpen)
                return;
            
            ToggleInteract(canInteract: false);
        }
    }
}