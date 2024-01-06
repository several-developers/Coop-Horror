using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerSprintConfig : ScriptableObject
    {
        [Header("Sprinting")]
        [Tooltip("True if player should be allowed to sprint.")]
        public bool allowSprinting = true;

        [Tooltip("Speed that player moves when sprinting.")]
        public float sprintSpeed = 9f;

        [Tooltip(
            "User may set sprint mode to toggle, hold, or both (toggle on sprint button press, hold on sprint button hold).")]
        public PlayerMovement.SprintType sprintMode = PlayerMovement.SprintType.Both;

        public bool forwardSprintOnly = true;
    }
}