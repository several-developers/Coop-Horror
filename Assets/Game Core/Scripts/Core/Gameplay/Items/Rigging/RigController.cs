using DG.Tweening;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.EntitiesSystems.Inventory;
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
        private TwoBoneIKConstraint _leftHandRig;

        [SerializeField, Required]
        private TwoBoneIKConstraint _rightHandRig;

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

            bool isDefaultPresetFound =
                _rigPresetsProvider.TryGetPresetMeta(RigPresetType.Default, out RigPresetMeta defaultRigPresetMeta);

            if (!isDefaultPresetFound)
            {
                Log.PrintError(log: $"Rig Preset of type <gb>{RigPresetType.Default}</gb> <rb>not found</rb>!");
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
                switch (rigType)
                {
                    case RigType.LeftHand:
                        leftHandTargetPose = rigPresetMeta.FPSLeftHandTargetPose;
                        leftHandHintPose = rigPresetMeta.FPSLeftHandHintPose;
                        rightHandTargetPose = defaultRigPresetMeta.FPSRightHandTargetPose;
                        rightHandHintPose = defaultRigPresetMeta.FPSRightHandHintPose;
                        break;
                    
                    case RigType.RightHand:
                        leftHandTargetPose = defaultRigPresetMeta.FPSLeftHandTargetPose;
                        leftHandHintPose = defaultRigPresetMeta.FPSLeftHandHintPose;
                        rightHandTargetPose = rigPresetMeta.FPSRightHandTargetPose;
                        rightHandHintPose = rigPresetMeta.FPSRightHandHintPose;
                        break;
                    
                    default:
                        leftHandTargetPose = rigPresetMeta.FPSLeftHandTargetPose;
                        rightHandTargetPose = rigPresetMeta.FPSRightHandTargetPose;
                        leftHandHintPose = rigPresetMeta.FPSLeftHandHintPose;
                        rightHandHintPose = rigPresetMeta.FPSRightHandHintPose;
                        break;
                }
            }
            else
            {
                switch (rigType)
                {
                    case RigType.LeftHand:
                        leftHandTargetPose = rigPresetMeta.TPSLeftHandTargetPose;
                        leftHandHintPose = rigPresetMeta.TPSLeftHandHintPose;
                        rightHandTargetPose = defaultRigPresetMeta.TPSRightHandTargetPose;
                        rightHandHintPose = defaultRigPresetMeta.TPSRightHandHintPose;
                        break;
                    
                    case RigType.RightHand:
                        leftHandTargetPose = defaultRigPresetMeta.TPSLeftHandTargetPose;
                        leftHandHintPose = defaultRigPresetMeta.TPSLeftHandHintPose;
                        rightHandTargetPose = rigPresetMeta.TPSRightHandTargetPose;
                        rightHandHintPose = rigPresetMeta.TPSRightHandHintPose;
                        break;
                    
                    default:
                        leftHandTargetPose = rigPresetMeta.TPSLeftHandTargetPose;
                        rightHandTargetPose = rigPresetMeta.TPSRightHandTargetPose;
                        leftHandHintPose = rigPresetMeta.TPSLeftHandHintPose;
                        rightHandHintPose = rigPresetMeta.TPSRightHandHintPose;
                        break;
                }
            }

            ChangeHandRig(
                handTarget: _leftHandTarget,
                handHint: _leftHandHint,
                handRig: _leftHandRig,
                targetPose: leftHandTargetPose,
                hintPose: leftHandHintPose,
                rigChangeDuration: rigPresetMeta.RigWeightChangeDuration,
                targetWeight: leftHandRigWeight);

            ChangeHandRig(
                handTarget: _rightHandTarget,
                handHint: _rightHandHint,
                handRig: _rightHandRig,
                targetPose: rightHandTargetPose,
                hintPose: rightHandHintPose,
                rigChangeDuration: rigPresetMeta.RigWeightChangeDuration,
                targetWeight: rightHandRigWeight);

            ChangeHandAnimatorLayer(rigPresetMeta, _leftHandLayerIndex, leftHandRigWeight);
            ChangeHandAnimatorLayer(rigPresetMeta, _rightHandLayerIndex, rightHandRigWeight);
        }

        private void ResetRig(bool isLocalPlayer) => UpdateRig(RigPresetType.Default, isLocalPlayer);

        private void ChangeHandRig(Transform handTarget, Transform handHint, TwoBoneIKConstraint handRig,
            RigPresetMeta.RigPose targetPose, RigPresetMeta.RigPose hintPose, float rigChangeDuration,
            float targetWeight)
        {
            FloatAnimation(from: handRig.weight, to: targetWeight, rigChangeDuration,
                onVirtualUpdate: weight => handRig.weight = weight);

            LocalMoveAnimation(handTarget, targetPose.Position, targetPose.Duration);
            LocalMoveAnimation(handHint, hintPose.Position, hintPose.Duration);
            LocalRotateAnimation(handTarget, targetPose.EulerRotation, targetPose.Duration);
            LocalRotateAnimation(handHint, hintPose.EulerRotation, hintPose.Duration);
        }

        private void ChangeHandAnimatorLayer(RigPresetMeta rigPresetMeta, int layerIndex, float targetWeight)
        {
            float from = _playersArmsAnimator.GetLayerWeight(layerIndex);
            float duration = rigPresetMeta.AnimatorLayerWeightChangeTime;

            FloatAnimation(from, to: targetWeight, duration,
                onVirtualUpdate: weight => _playersArmsAnimator.SetLayerWeight(layerIndex, weight));
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