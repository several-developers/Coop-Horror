﻿using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.HUD.PlayerHealth
{
    public class PlayerHealthUI : UIElement
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _healthTMP;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateHealthText(HealthData healthData)
        {
            float currentHealth = healthData.CurrentHealth;
            float maxHealth = healthData.MaxHealth;
            _healthTMP.text = $"Health: {currentHealth:F0}/{maxHealth:F0}";
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            HealthSystem healthSystem = playerEntity.References.HealthSystem;
            HealthData healthData = healthSystem.GetHealthData();
            UpdateHealthText(healthData);
            
            healthSystem.OnHealthChangedEvent += OnHealthChanged;

            playerEntity.OnDiedEvent += OnPlayerDied;
            playerEntity.OnRevivedEvent += OnPlayerRevived;
        }

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;
            
            HealthSystem healthSystem = playerEntity.References.HealthSystem;
            healthSystem.OnHealthChangedEvent -= OnHealthChanged;
            
            playerEntity.OnDiedEvent -= OnPlayerDied;
            playerEntity.OnRevivedEvent -= OnPlayerRevived;
        }

        private void OnHealthChanged(HealthData healthData) => UpdateHealthText(healthData);
        
        private void OnPlayerDied() => Hide();

        private void OnPlayerRevived() => Show();
    }
}