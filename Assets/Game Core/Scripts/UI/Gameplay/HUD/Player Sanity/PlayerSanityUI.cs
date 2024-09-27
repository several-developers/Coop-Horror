using GameCore.Gameplay.Entities.Player;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.HUD.PlayerSanity
{
    public class PlayerSanityUI : UIElement
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _sanityTMP;

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
            playerEntity.OnDeathEvent += OnPlayerDeath;
            playerEntity.OnRevivedEvent += OnPlayerRevived;
        }

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            playerEntity.OnSanityChangedEvent -= OnSanityChanged;
            playerEntity.OnDeathEvent -= OnPlayerDeath;
            playerEntity.OnRevivedEvent -= OnPlayerRevived;
        }

        private void OnSanityChanged(float sanity) => UpdateSanityText(sanity);

        private void OnPlayerDeath() => Hide();

        private void OnPlayerRevived(PlayerEntity _) => Show();
    }
}