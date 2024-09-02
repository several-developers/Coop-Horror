using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Interactable;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Entities.Interactable.Outdoor_Chest
{
    public class OutdoorChestEntity : Entity, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsFactory itemsFactory) =>
            _itemsFactory = itemsFactory;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private Collider _triggerCollider;

        [SerializeField, Required, Space(height: 5)]
        private List<Transform> _itemsSpawnPoints;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent = delegate { };

        private readonly NetworkVariable<bool> _isOpen = new(writePerm: Constants.OwnerPermission);
        
        private IItemsFactory _itemsFactory;

        private List<int> _itemsList;
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
            _triggerCollider.enabled = canInteract;
            
            OnInteractionStateChangedEvent.Invoke();
        }

        public void SetupItemsList(List<int> itemsList) =>
            _itemsList = itemsList;

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
            
            SpawnItems();
        }

        private void SpawnItems()
        {
            bool isItemsListValid = _itemsList != null && _itemsList.Count != 0;

            if (!isItemsListValid)
            {
                Log.PrintError(log: "Chest is empty!");
                return;
            }

            foreach (int itemID in _itemsList)
            {
                Transform itemSpawnPoint = GetRandomItemSpawnPoint();
                bool isSpawnPointValid = itemSpawnPoint != null;

                if (!isSpawnPointValid)
                {
                    Log.PrintError(log: "Spawn Point is not valid!");
                    return;
                }

                Vector3 worldPosition = itemSpawnPoint.position;
                _itemsFactory.CreateItem(itemID, worldPosition, out _);
            }
            
            _itemsList.Clear();
            _itemsList = null;
        }

        private Transform GetRandomItemSpawnPoint()
        {
            int randomIndex = Random.Range(0, _itemsSpawnPoints.Count);
            return _itemsSpawnPoints[randomIndex];
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