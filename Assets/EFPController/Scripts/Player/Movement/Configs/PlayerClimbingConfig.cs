using GameCore;
using CustomEditors;
using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerClimbingConfig : EditorMeta
    {
        [Header("Climbing")]
        public bool allowClimbing = true;

        [Tooltip("Speed that player moves vertically when climbing.")]
        public float climbingSpeed = 5.0f;

        public float angleToClimb = 45f;
        public float grabbingTime = 0.25f;
        public bool clampClimbingCameraRotate = true;
        public float clampClimbingCameraAngle = 45f;
        public bool rotateClimbingCamera = true;
        public bool climbOnInteract = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}