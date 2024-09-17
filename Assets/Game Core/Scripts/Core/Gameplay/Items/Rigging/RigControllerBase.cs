using System;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Inventory;
using GameCore.Infrastructure.Providers.Gameplay.RigPresets;
using GameCore.Infrastructure.Providers.Global.ItemsMeta;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Zenject;

namespace GameCore.Gameplay.Items.Rigging
{
    public class RigControllerBase : MonoBehaviour
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
        
        private Animator _animator;
        private Sequence _sequence;
        private int _leftHandLayerIndex;
        private int _rightHandLayerIndex;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetAnimator(Animator animator)
        {
            _animator = animator;

            _leftHandLayerIndex = _animator.GetLayerIndex("Left Hand");
            _rightHandLayerIndex = _animator.GetLayerIndex("Right Hand");
        }

        public void TryUpdateRig(ulong clientID)
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
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void ResetRig(bool isLocalPlayer) => UpdateRig(RigPresetType.Default, isLocalPlayer);
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

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
            float from = _animator.GetLayerWeight(layerIndex);
            float duration = rigPresetMeta.AnimatorLayerWeightChangeTime;

            FloatAnimation(from, to: targetWeight, duration,
                onVirtualUpdate: weight => _animator.SetLayerWeight(layerIndex, weight));
        }

        private void FloatAnimation(float from, float to, float duration, TweenCallback<float> onVirtualUpdate) =>
            _sequence.Join(DOVirtual.Float(from, to, duration, onVirtualUpdate));

        private void LocalMoveAnimation(Transform target, Vector3 position, float duration) =>
            _sequence.Join(target.DOLocalMove(position, duration));

        private void LocalRotateAnimation(Transform target, Vector3 eulerRotation, float duration) =>
            _sequence.Join(target.DOLocalRotate(eulerRotation, duration));
    }
}