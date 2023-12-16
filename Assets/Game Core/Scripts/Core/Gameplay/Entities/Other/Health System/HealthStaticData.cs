namespace GameCore.Gameplay.Entities.Other
{
    public struct HealthStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public HealthStaticData(float currentHealth, float maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public float CurrentHealth { get; }
        public float MaxHealth { get; }
    }
}