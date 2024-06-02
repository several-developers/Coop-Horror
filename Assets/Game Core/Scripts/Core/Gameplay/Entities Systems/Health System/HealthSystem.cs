using System;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Health
{
    public class HealthSystem : NetcodeBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.DebugInfo)]
        [SerializeField, ReadOnly]
        private float _maxHealthDebug;
        
        [SerializeField, ReadOnly]
        private float _currentHealthDebug;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<HealthData> OnHealthChangedEvent = delegate { }; 

        private readonly NetworkVariable<HealthData> _currentHealth = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(float maxHealth)
        {
            if (!IsOwner)
                return;
                
            HealthData healthData = new(maxHealth);
            _currentHealth.Value = healthData;
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsSpawned)
                return;
            
            if (IsOwner)
                TakeDamageLogic(damage);
            else
                TakeDamageServerRpc(damage);
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll() =>
            _currentHealth.OnValueChanged += OnHealthChangedDebug;

        protected override void DespawnAll() =>
            _currentHealth.OnValueChanged -= OnHealthChangedDebug;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TakeDamageLogic(float damage)
        {
            HealthData healthData = _currentHealth.Value;
            float newHealth = Mathf.Max(a: healthData.CurrentHealth - damage, b: 0f);
            healthData.SetCurrentHealth(newHealth);
            
            _currentHealth.Value = healthData;
            OnHealthChangedEvent.Invoke(healthData);
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage) => TakeDamageLogic(damage);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHealthChangedDebug(HealthData previousValue, HealthData newValue)
        {
            _maxHealthDebug = newValue.MaxHealth;
            _currentHealthDebug = newValue.CurrentHealth;
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugSetup(float maxHealth) => Setup(maxHealth);
        
        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugTakeDamage(float damage) => TakeDamage(damage);
    }
}