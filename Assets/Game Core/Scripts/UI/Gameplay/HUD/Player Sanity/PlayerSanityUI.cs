using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.GameManagement;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.PlayerSanity
{
    public class PlayerSanityUI : UIElement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

            [Inject]
            private void Construct(IGameManagerDecorator gameManagerDecorator) =>
                _gameManagerDecorator = gameManagerDecorator;

            // MEMBERS: -------------------------------------------------------------------------------

            [Title(Constants.References)]
            [SerializeField, Required]
            private TextMeshProUGUI _sanityTMP;

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

            private void UpdateSanityText(float sanity) =>
                _sanityTMP.text = $"Sanity: {sanity:F0}%";

            // EVENTS RECEIVERS: ----------------------------------------------------------------------

            private void OnPlayerSpawned(PlayerEntity playerEntity)
            {
                bool isLocalPlayer = playerEntity.IsLocalPlayer();

                if (!isLocalPlayer)
                    return;

                float sanity = playerEntity.GetSanity();
                UpdateSanityText(sanity);
                
                playerEntity.OnSanityChangedEvent += OnSanityChanged;
                playerEntity.OnDiedEvent += OnPlayerDied;
                playerEntity.OnRevivedEvent += OnPlayerRevived;
            }

            private void OnPlayerDespawned(PlayerEntity playerEntity)
            {
                bool isLocalPlayer = playerEntity.IsLocalPlayer();

                if (!isLocalPlayer)
                    return;

                playerEntity.OnSanityChangedEvent -= OnSanityChanged;
                playerEntity.OnDiedEvent -= OnPlayerDied;
                playerEntity.OnRevivedEvent -= OnPlayerRevived;
            }

            private void OnSanityChanged(float sanity) => UpdateSanityText(sanity);

            private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

            private void OnPlayerDied() => Hide();

            private void OnPlayerRevived() => Show();
    }
}