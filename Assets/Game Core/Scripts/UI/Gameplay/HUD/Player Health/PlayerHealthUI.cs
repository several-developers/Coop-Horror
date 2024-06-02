using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.Gameplay.GameManagement;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.PlayerHealth
{
    public class PlayerHealthUI : UIElement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _healthTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
            
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
            
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.KillPlayersOnTheRoad:
                    Hide();
                    break;
                
                case GameState.RestartGame:
                    Show();
                    break;
            }
        }

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

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnPlayerDied() => Hide();

        private void OnPlayerRevived() => Show();
    }
}