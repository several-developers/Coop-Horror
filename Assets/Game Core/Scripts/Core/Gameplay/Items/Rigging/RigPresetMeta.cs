using System;
using CustomEditors;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Items.Rigging
{
    public class RigPresetMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private RigType _rigType;

        [SerializeField, Min(0)]
        [Tooltip("Blend time when changing rig weight.\n1 unit = 1 second.")]
        private float _rigWeightChangeDuration = 0.3f;
        
        [SerializeField, Min(0)]
        [Tooltip("Blend time when changing layer weight in the animator.\n1 unit = 1 second.")]
        private float _animatorLayerWeightChangeTime = 0.3f;
        
        [SerializeField]
        [ShowIf(nameof(ShowLeftHand))]
        private RigPose _fpsLeftHandTargetPose;
        
        [SerializeField]
        [ShowIf(nameof(ShowRightHand))]
        private RigPose _fpsRightHandTargetPose;
        
        [SerializeField]
        [ShowIf(nameof(ShowLeftHand))]
        private RigPose _fpsLeftHandHintPose;
        
        [SerializeField]
        [ShowIf(nameof(ShowRightHand))]
        private RigPose _fpsRightHandHintPose;

        // PROPERTIES: ----------------------------------------------------------------------------

        public RigType RigType => _rigType;
        public float RigWeightChangeDuration => _rigWeightChangeDuration;
        public float AnimatorLayerWeightChangeTime => _animatorLayerWeightChangeTime;
        public RigPose FPSLeftHandTargetPose => _fpsLeftHandTargetPose;
        public RigPose FPSRightHandTargetPose => _fpsRightHandTargetPose;
        public RigPose FPSLeftHandHintPose => _fpsLeftHandHintPose;
        public RigPose FPSRightHandHintPose => _fpsRightHandHintPose;

        private bool ShowLeftHand => _rigType is RigType.LeftHand or RigType.BothHands or RigType.None;
        private bool ShowRightHand => _rigType is RigType.RightHand or RigType.BothHands or RigType.None;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.RigPresetsCategory;
        
        [Serializable]
        public class RigPose
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField]
            private Vector3 _position;
        
            [SerializeField]
            private Vector3 _eulerRotation;
        
            [SerializeField, Min(0)]
            [SuffixLabel("seconds", overlay: true)]
            private float _duration = 0.3f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Vector3 Position => _position;
            public Vector3 EulerRotation => _eulerRotation;
            public float Duration => _duration;
        }
    }
}