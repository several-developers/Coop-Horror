using DG.Tweening;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
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
        private void Construct(IItemsMetaProvider itemsMetaProvider, IRigPresetsProvider rigPresetsProvider,
            PlayerCamera playerCamera)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _rigPresetsProvider = rigPresetsProvider;
            
            CameraReferences cameraReferences = playerCamera.CameraReferences;
            _playersArmsAnimator = cameraReferences.PlayerArmsAnimator;
            
            _leftHandLayerIndex = _playersArmsAnimator.GetLayerIndex("Left Hand");
            _rightHandLayerIndex = _playersArmsAnimator.GetLayerIndex("Right Hand");
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
        private Animator _playersArmsAnimator;

        private Sequence _sequence;

        private int _leftHandLayerIndex;
        private int _rightHandLayerIndex;

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

        private void TryUpdateRig(ulong clientID)
        {
            bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);

            if (!isPlayerFound)
                return;

            PlayerInventory playerInventory = playerEntity.GetInventory();
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            bool isItemInSelectedSlotExists =
                playerInventory.TryGetSelectedItemData(out InventoryItemData inventoryItemData);

            if (!isItemInSelectedSlotExists)
            {
                ResetRig(isLocalPlayer);
                return;
            }

            int itemID = inventoryItemData.ItemID;
            bool isItemMetaFound = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaFound)
            {
                ResetRig(isLocalPlayer);
                return;
            }

            RigPresetType presetType = itemMeta.RigPresetType;
            UpdateRig(presetType, isLocalPlayer);
        }

        private void UpdateRig(RigPresetType presetType, bool isLocalPlayer)
        {
            bool isPresetFound = _rigPresetsProvider.TryGetPresetMeta(presetType, out RigPresetMeta rigPresetMeta);

            if (!isPresetFound)
            {
                Log.PrintError(log: $"Rig Preset of type <gb>{presetType}</gb> <rb>not found</rb>!");
                return;
            }
            
            _sequence.Kill();
            _sequence = DOTween.Sequence();
            _sequence.SetLink(gameObject);

            RigType rigType = rigPresetMeta.RigType;
            bool showLeftHand = rigType is RigType.LeftHand or RigType.BothHands;
            bool showRightHand = rigType is RigType.RightHand or RigType.BothHands;
            float leftHandRigWeight = showLeftHand ? 1f : 0f;
            float rightHandRigWeight = showRightHand ? 1f : 0f;
            
            RigPresetMeta.RigPose leftHandTargetPose;
            RigPresetMeta.RigPose rightHandTargetPose;
            RigPresetMeta.RigPose leftHandHintPose;
            RigPresetMeta.RigPose rightHandHintPose;

            if (isLocalPlayer)
            {
                leftHandTargetPose = rigPresetMeta.FPSLeftHandTargetPose;
                rightHandTargetPose = rigPresetMeta.FPSRightHandTargetPose;
                leftHandHintPose = rigPresetMeta.FPSLeftHandHintPose;
                rightHandHintPose = rigPresetMeta.FPSRightHandHintPose;
            }
            else
            {
                leftHandTargetPose = rigPresetMeta.FPSLeftHandTargetPose;
                rightHandTargetPose = rigPresetMeta.FPSRightHandTargetPose;
                leftHandHintPose = rigPresetMeta.FPSLeftHandHintPose;
                rightHandHintPose = rigPresetMeta.FPSRightHandHintPose;
            }
            
            ChangeRightHandRig(targetPose: rightHandTargetPose, hintPose: rightHandHintPose,
                rigChangeDuration: rigPresetMeta.RigWeightChangeDuration, targetWeight: rightHandRigWeight);
            
            ChangeRightHandAnimatorLayer(rigPresetMeta, rightHandRigWeight);
        }

        private void ResetRig(bool isLocalPlayer) => UpdateRig(RigPresetType.None, isLocalPlayer);

        private void ChangeRightHandRig(RigPresetMeta.RigPose targetPose, RigPresetMeta.RigPose hintPose,
            float rigChangeDuration, float targetWeight)
        {
            FloatAnimation(from: _rightHandRig.weight, to: targetWeight, rigChangeDuration,
                onVirtualUpdate: weight => _rightHandRig.weight = weight);

            LocalMoveAnimation(_rightHandTarget, targetPose.Position, targetPose.Duration);
            LocalMoveAnimation(_rightHandHint, hintPose.Position, hintPose.Duration);
            LocalRotateAnimation(_rightHandTarget, targetPose.EulerRotation, targetPose.Duration);
            LocalRotateAnimation(_rightHandHint, hintPose.EulerRotation, hintPose.Duration);
        }

        private void ChangeRightHandAnimatorLayer(RigPresetMeta rigPresetMeta, float targetWeight)
        {
            float from = _playersArmsAnimator.GetLayerWeight(_rightHandLayerIndex);
            float duration = rigPresetMeta.AnimatorLayerWeightChangeTime;
            
            FloatAnimation(from, to: targetWeight, duration,
                onVirtualUpdate: weight => _playersArmsAnimator.SetLayerWeight(_rightHandLayerIndex, weight));
        }

        private void FloatAnimation(float from, float to, float duration, TweenCallback<float> onVirtualUpdate) =>
            _sequence.Join(DOVirtual.Float(from, to, duration, onVirtualUpdate));

        private void LocalMoveAnimation(Transform target, Vector3 position, float duration) =>
            _sequence.Join(target.DOLocalMove(position, duration));

        private void LocalRotateAnimation(Transform target, Vector3 eulerRotation, float duration) =>
            _sequence.Join(target.DOLocalRotate(eulerRotation, duration));

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