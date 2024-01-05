using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerLeaningConfig : ScriptableObject
    {
        [Header("Leaning")]
        [Tooltip("True if player should be allowed to lean.")]
        public bool allowLeaning = true;

        [Tooltip("Distance left or right the player can lean.")]
        public float leanDistance = 0.75f;

        [Tooltip("Percentage the player can lean while standing.")]
        [Range(0, 1)]
        public float standLeanAmt = 1f;

        [Tooltip("Percentage the player can lean while crouching.")]
        [Range(0, 1)]
        public float crouchLeanAmt = 1f;

        public float rotationLeanAmt = 20f;
    }
}