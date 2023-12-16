using System;
using GameCore.Gameplay.Entities.Other.DamageReceivers;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Other
{
    public class SimpleHealthSystem : IHealthSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SimpleHealthSystem(DamageReceiver damageReceiver)
        {
            _maxHealth = 100;
            _currentHealth = _maxHealth;

            damageReceiver.OnDamageReceivedEvent += OnDamageReceived;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<HealthStaticData> OnHealthUpdatedEvent;
        public event Action OnDeathEvent;

        private readonly float _maxHealth;
        
        private float _currentHealth;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Max(_currentHealth - damage, 0);
            bool isDead = _currentHealth <= 0;

            SendHealthUpdatedEvent();
            
            if (!isDead)
                return;

            OnDeathEvent?.Invoke();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendHealthUpdatedEvent()
        {
            HealthStaticData healthData = new(_currentHealth, _maxHealth);
            OnHealthUpdatedEvent?.Invoke(healthData);
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnDamageReceived(DamageStaticData damageData)
        {
            float damage = damageData.Damage;
            TakeDamage(damage);
        }
    }
}