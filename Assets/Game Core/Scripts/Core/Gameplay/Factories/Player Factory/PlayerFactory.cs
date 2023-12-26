using GameCore.Core.Configs.Player;
using GameCore.Gameplay.Entities.Player;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Player
{
    public class PlayerFactory : IPlayerFactory
    {
        public PlayerFactory(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _playerConfig = gameplayConfigsProvider.GetPlayerConfig();
        }

        private readonly PlayerConfigMeta _playerConfig;

        public void Create(Vector3 at)
        {
            PlayerEntity playerPrefab = _playerConfig.PlayerPrefab;
            PlayerEntity playerInstance = Object.Instantiate(playerPrefab, at, Quaternion.identity);

            //playerInstance.Setup();
        }
    }
}