using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.Infrastructure.Providers.Gameplay.RigPresets;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Zenject;

namespace GameCore.Gameplay.Items.Rigging
{
    public class RigController : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsMetaProvider itemsMetaProvider, IRigPresetsProvider rigPresetsProvider)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _rigPresetsProvider = rigPresetsProvider;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Rig _leftHandRig;

        [SerializeField, Required]
        private Rig _rightHandRig;

        [SerializeField, Required]
        private Transform _leftHandTarget;

        [SerializeField, Required]
        private Transform _leftHandHint;

        [SerializeField, Required]
        private Transform _rightHandTarget;

        [SerializeField, Required]
        private Transform _rightHandHint;

        // FIELDS: --------------------------------------------------------------------------------

        private IItemsMetaProvider _itemsMetaProvider;
        private IRigPresetsProvider _rigPresetsProvider;
        private PlayerInventory _playerInventory;

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryUpdateRig()
        {
            bool isItemInSelectedSlotExists =
                _playerInventory.TryGetSelectedItemData(out InventoryItemData inventoryItemData);

            if (!isItemInSelectedSlotExists)
            {
                ResetRig();
                return;
            }

            int itemID = inventoryItemData.ItemID;
            bool isItemMetaFound = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaFound)
            {
                ResetRig();
                return;
            }

            RigPresetType presetType = itemMeta.RigPresetType;
            
            if (presetType == RigPresetType.None)
                ResetRig();
            else
                UpdateRig(presetType);
        }

        private void UpdateRig(RigPresetType presetType)
        {
            bool isPresetFound = _rigPresetsProvider.TryGetPresetMeta(presetType, out RigPresetMeta rigPresetMeta);

            if (!isPresetFound)
            {
                Log.PrintError(log: $"Rig Preset of type <gb>{presetType}</gb> <rb>not found</rb>!");
                return;
            }

            RigType rigType = rigPresetMeta.RigType;
            float leftHandRigWeight = rigType is RigType.LeftHand or RigType.BothHands ? 1f : 0f;
            float rightHandRigWeight = rigType is RigType.RightHand or RigType.BothHands ? 1f : 0f;

            //_leftHandRig.weight = leftHandRigWeight;
            _rightHandRig.weight = rightHandRigWeight;

            RigPresetMeta.RigPose fpsTargetPose = rigPresetMeta.FPSTargetPose;
            RigPresetMeta.RigPose fpsHintPose = rigPresetMeta.FPSHintPose;

            _rightHandTarget.localPosition = fpsTargetPose.Position;
            _rightHandTarget.localRotation = Quaternion.Euler(fpsTargetPose.EulerRotation);

            _rightHandHint.localPosition = fpsHintPose.Position;
            _rightHandHint.localRotation = Quaternion.Euler(fpsHintPose.EulerRotation);
        }

        private void ResetRig()
        {
            //_leftHandRig.weight = 0f;
            _rightHandRig.weight = 0f;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayerSpawned()
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            _playerInventory = localPlayer.GetInventory();

            _playerInventory.OnSelectedSlotChangedEvent += OnInventorySelectedSlotChanged;
        }

        private void OnPlayerDespawned() =>
            _playerInventory.OnSelectedSlotChangedEvent -= OnInventorySelectedSlotChanged;

        private void OnInventorySelectedSlotChanged(int slotIndex) => TryUpdateRig();
    }
}