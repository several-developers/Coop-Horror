using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerFallDamageConfig : ScriptableObject
    {
        [Header("Fall Damage")]
        public bool allowFallDamage = true;

        [Tooltip("Number of units the player can fall before taking damage.")]
        public float fallDamageThreshold = 5.5f;

        [Tooltip("Multiplier of unit distance fallen to convert to hitpoint damage for the player.")]
        public float fallDamageMultiplier = 2f;
    }
}