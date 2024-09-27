using UnityEngine;

namespace GameCore.Gameplay.Network.UnityServices.Other
{
    public class RateLimitCooldown
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public RateLimitCooldown(float cooldownTimeLength)
        {
            _cooldownTimeLength = cooldownTimeLength;
            _cooldownFinishedTime = -1f;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public bool CanCall => Time.unscaledTime > _cooldownFinishedTime;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly float _cooldownTimeLength;
        
        private float _cooldownFinishedTime;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PutOnCooldown() =>
            _cooldownFinishedTime = Time.unscaledTime + _cooldownTimeLength;
    }
}