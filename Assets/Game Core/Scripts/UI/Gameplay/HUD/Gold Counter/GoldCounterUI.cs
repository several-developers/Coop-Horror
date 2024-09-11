using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.GameManagement;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.GoldCounter
{
    public class GoldCounterUI : UIElement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _goldTMP;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameManagerDecorator _gameManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
            
            _gameManagerDecorator.OnPlayersGoldChangedEvent += OnPlayersGoldChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
            
            _gameManagerDecorator.OnPlayersGoldChangedEvent -= OnPlayersGoldChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateGoldText(int playersGold) =>
            _goldTMP.text = $"Gold: {playersGold}";

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnPlayersGoldChanged(int playersGold) => UpdateGoldText(playersGold);
        
        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            playerEntity.OnDeathEvent += OnPlayerDeath;
            playerEntity.OnRevivedEvent += OnPlayerRevived;
        }

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;
            
            playerEntity.OnDeathEvent -= OnPlayerDeath;
            playerEntity.OnRevivedEvent -= OnPlayerRevived;
        }
        
        private void OnPlayerDeath() => Hide();

        private void OnPlayerRevived() => Show();
    }
}