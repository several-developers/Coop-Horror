using GameCore;
using CustomEditors;
using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerJumpingConfig : EditorMeta
    {
        [Header("Jumping")]
        [Tooltip("True if player should be allowed to jump.")]
        public bool allowJumping = true;

        [Tooltip("Vertical speed of player jump.")]
        public float jumpSpeed = 4f;

        [Tooltip("True if player can Jump while sprinting and not cancel sprint.")]
        public bool jumpCancelsSprint;

        [Tooltip("Time in seconds allowed between player jumps.")]
        public float antiBunnyHopFactor = 0.1f;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}