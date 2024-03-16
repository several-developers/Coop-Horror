using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.Entities.Player;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Player
{
    public class PlayerFactory : IPlayerFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public PlayerFactory(IGameplayConfigsProvider gameplayConfigsProvider) =>
            _playerConfig = gameplayConfigsProvider.GetPlayerConfig();

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly PlayerConfigMeta _playerConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public PlayerEntity Create(Vector3 at)
        {
            PlayerEntity playerPrefab = _playerConfig.PlayerPrefab;
            PlayerEntity playerInstance = Object.Instantiate(playerPrefab, at, Quaternion.identity);
            return playerInstance;
        }
    }
}