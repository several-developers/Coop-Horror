using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerSwimmingConfig : ScriptableObject
    {
        [Header("Swimming")]
        [Tooltip("Speed that player moves when swimming.")]
        public float swimSpeed = 4f;

        [Tooltip("Amount of time before player starts drowning.")]
        public float holdBreathDuration = 15f;

        [Tooltip("Rate of damage to player while drowning.")]
        public float drownDamage = 7f;

        public float swimmingCamHeight = -0.2f;
        public float swimmingCapsuleHeight = 1.25f; // height of capsule while crouching
    }
}