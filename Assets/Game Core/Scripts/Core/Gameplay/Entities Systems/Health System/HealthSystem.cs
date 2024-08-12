using System;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Health
{
    public class HealthSystem : NetcodeBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.DebugInfo)]
        [SerializeField, ReadOnly]
        private float _maxHealthInfo;
        
        [SerializeField, ReadOnly]
        private float _currentHealthInfo;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<HealthData> OnHealthChangedEvent = delegate { }; 

        private readonly NetworkVariable<HealthData> _healthData = new(writePerm: Constants.OwnerPermission);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(float maxHealth)
        {
            if (!IsOwner)
                return;
                
            HealthData healthData = new(maxHealth);
            SetHealth(healthData);
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

        public void Kill() => TakeDamage(damage: 10000f);

        public void Reset()
        {
            HealthData healthData = GetHealthData();
            float maxHealth = healthData.MaxHealth;
            healthData.SetCurrentHealth(maxHealth);

            SetHealth(healthData);
        }
        
        public HealthData GetHealthData() =>
            _healthData.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll() =>
            _healthData.OnValueChanged += OnHealthChangedDebug;

        protected override void DespawnAll() =>
            _healthData.OnValueChanged -= OnHealthChangedDebug;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TakeDamageLogic(float damage)
        {
            HealthData healthData = GetHealthData();
            float newHealth = Mathf.Max(a: healthData.CurrentHealth - damage, b: 0f);
            healthData.SetCurrentHealth(newHealth);

            SetHealth(healthData);
        }

        private void SetHealth(HealthData healthData)
        {
            _healthData.Value = healthData;
            OnHealthChangedEvent.Invoke(healthData);
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage) => TakeDamageLogic(damage);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHealthChangedDebug(HealthData previousValue, HealthData newValue)
        {
            _maxHealthInfo = newValue.MaxHealth;
            _currentHealthInfo = newValue.CurrentHealth;
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugSetup(float maxHealth) => Setup(maxHealth);
        
        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugTakeDamage(float damage) => TakeDamage(damage);
    }
}