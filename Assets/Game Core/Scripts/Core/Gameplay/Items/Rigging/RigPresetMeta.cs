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
        
        [Title("First Person Settings")]
        [SerializeField]
        private RigPose _fpsTargetPose;
        
        [SerializeField]
        private RigPose _fpsHintPose;

        // PROPERTIES: ----------------------------------------------------------------------------

        public RigType RigType => _rigType;
        public RigPose FPSTargetPose => _fpsTargetPose;
        public RigPose FPSHintPose => _fpsHintPose;
        
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
            [Tooltip("Blend time when transitioning into this pose.\n1 unit = 1 second.")]
            private float _transformSmoothDampTime = 0.1f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Vector3 Position => _position;
            public Vector3 EulerRotation => _eulerRotation;
            public float TransformSmoothDampTime => _transformSmoothDampTime;
        }
    }
}