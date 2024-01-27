using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class JumpComponentConfig : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        [Tooltip("Vertical speed of player jump.")]
        private float _jumpSpeed = 6f;

        [SerializeField, Min(0)]
        [Tooltip("Time in seconds allowed between player jumps.")]
        private float _antiBunnyHopFactor = 0.1f;

        [SerializeField]
        [Tooltip("True if player can Jump while sprinting and not cancel sprint.")]
        private bool _jumpCancelsSprint;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float JumpSpeed => _jumpSpeed;
        public float AntiBunnyHopFactor => _antiBunnyHopFactor;
        public bool JumpCancelsSprint => _jumpCancelsSprint;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory + "/Movement";
    }
}