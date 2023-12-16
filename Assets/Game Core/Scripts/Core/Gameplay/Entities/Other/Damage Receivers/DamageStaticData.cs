namespace GameCore.Gameplay.Entities.Other.DamageReceivers
{
    public struct DamageStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public DamageStaticData(float damage)
        {
            Source = null;
            Damage = damage;
        }

        public DamageStaticData(IEntity source, float damage)
        {
            Source = source;
            Damage = damage;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public IEntity Source { get; }
        public float Damage { get; }
    }
}