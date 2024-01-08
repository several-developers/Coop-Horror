using GameCore;
using MetaEditor;
using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerCrouchingConfig : EditorMeta
    {
        [Header("Crouching")]
        [Tooltip("True if player should be allowed to crouch.")]
        public bool allowCrouch = true;

        [Tooltip("Percentage to decrease movement speed when crouching.")]
        [Range(0, 1)]
        public float crouchSpeedMult = 0.55f; // percentage to decrease movement speed while crouchin

        public float crouchCooldown = 0.57f;

        [Tooltip("Y position/height of camera when crouched.")]
        public float crouchingCamHeight = 0.45f;

        [Tooltip("Height of player capsule while crouching.")]
        public float crouchCapsuleHeight = 1.25f; // height of capsule while crouching

        public float
            crouchCapsuleCastHeight =
                0.80f; // height of capsule cast above player to check for obstacles before standing from crouch

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}