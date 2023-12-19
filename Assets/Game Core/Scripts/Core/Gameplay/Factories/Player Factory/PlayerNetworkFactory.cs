using GameCore.Core.Configs.Player;
using GameCore.Gameplay.Entities.Player;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Player
{
    public class PlayerNetworkFactory : IPlayerFactory
    {
        public PlayerNetworkFactory(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _playerConfig = gameplayConfigsProvider.GetPlayerConfig();
        }

        private readonly PlayerConfigMeta _playerConfig;

        public void Create(Vector3 at)
        {
            PlayerEntity playerPrefab = _playerConfig.PlayerPrefab;
            PlayerEntity playerInstance = Object.Instantiate(playerPrefab, at, Quaternion.identity);
            //NetworkObject playerNetworkObject = playerInstance.GetNetworkObject();
            NetworkObject playerNetworkObject = null;

            playerInstance.Setup();
            playerNetworkObject.Spawn();
        }
    }
}