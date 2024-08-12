using Unity.Netcode;

namespace GameCore.Gameplay.Systems.Health
{
    public struct HealthData : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public HealthData(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;

        // FIELDS: --------------------------------------------------------------------------------

        private float _maxHealth;
        private float _currentHealth;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _maxHealth);
            serializer.SerializeValue(ref _currentHealth);
        }

        public void SetCurrentHealth(float value) =>
            _currentHealth = value;
    }
}