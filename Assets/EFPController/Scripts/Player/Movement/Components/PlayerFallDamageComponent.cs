namespace EFPController
{
    public class PlayerFallDamageComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerFallDamageComponent(Player player, PlayerFallDamageConfig fallDamageConfig)
        {
            _fallDamageConfig = fallDamageConfig;
            _playerMovement = player.controller;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerFallDamageConfig FallDamageConfig => _fallDamageConfig;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly PlayerMovement _playerMovement;
        private readonly PlayerFallDamageConfig _fallDamageConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void CalculateFallingDamage(float fallDistance)
        {
            float fallDamageMultiplier = _fallDamageConfig.fallDamageMultiplier;

            if (fallDamageMultiplier < 1f)
                return;
            
            int damage = (int)(fallDistance * fallDamageMultiplier);
            _playerMovement.SendFallDamageEvent(damage);
        }
    }
}