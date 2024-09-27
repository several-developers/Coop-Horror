using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Systems.Inventory;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Items.Rigging
{
    public class FirstPersonRigController : RigControllerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(PlayerCamera playerCamera)
        {
            CameraReferences cameraReferences = playerCamera.CameraReferences;
            Animator playersArmsAnimator = cameraReferences.PlayerArmsAnimator;

            SetAnimator(playersArmsAnimator);
        }
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
        }

        private void OnDestroy()
        {
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            ResetRig(isLocalPlayer: true);

            PlayerInventory playerInventory = playerEntity.GetInventory();

            playerInventory.OnItemEquippedEvent += OnItemEquipped;
            playerInventory.OnItemDroppedEvent += OnItemDropped;
            playerInventory.OnSelectedSlotChangedEvent += OnInventorySelectedSlotChanged;
        }

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            PlayerInventory playerInventory = playerEntity.GetInventory();

            playerInventory.OnItemEquippedEvent -= OnItemEquipped;
            playerInventory.OnItemDroppedEvent -= OnItemDropped;
            playerInventory.OnSelectedSlotChangedEvent -= OnInventorySelectedSlotChanged;
        }
        
        private void OnItemEquipped(EquippedItemStaticData data) => TryUpdateRig(data.ClientID);

        private void OnItemDropped(DroppedItemStaticData data) => TryUpdateRig(data.ClientID);

        private void OnInventorySelectedSlotChanged(ChangedSlotStaticData data) => TryUpdateRig(data.ClientID);
    }
}