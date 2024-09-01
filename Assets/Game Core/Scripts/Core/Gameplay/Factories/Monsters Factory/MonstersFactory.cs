using System.Collections.Generic;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories.Monsters
{
    public class MonstersFactory : IMonstersFactory, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersFactory(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _gameplayConfigsProvider = gameplayConfigsProvider;
            _networkManager = NetworkManager.Singleton;
            _monstersPrefabs = new Dictionary<MonsterType, MonsterEntityBase>();
            _serverID = NetworkHorror.ServerID;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameplayConfigsProvider _gameplayConfigsProvider;
        private readonly NetworkManager _networkManager;
        private readonly Dictionary<MonsterType, MonsterEntityBase> _monstersPrefabs;
        private readonly ulong _serverID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize() => SetupMonstersPrefabsDictionary();

        public bool SpawnMonster(
            MonsterType monsterType,
            Vector3 worldPosition,
            Quaternion rotation,
            out MonsterEntityBase monsterEntity
        )
        {
            bool isPrefabFound = TryGetPrefabNetworkObject(monsterType, out NetworkObject prefabNetworkObject);

            if (!isPrefabFound)
            {
                monsterEntity = null;
                return false;
            }
            
            NetworkObject networkObject = _networkManager.SpawnManager
                .InstantiateAndSpawn(prefabNetworkObject, _serverID, destroyWithScene: true, position: worldPosition);

            monsterEntity = networkObject.GetComponent<MonsterEntityBase>();

            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetupMonstersPrefabsDictionary()
        {
            MonstersListConfigMeta monstersListConfig = _gameplayConfigsProvider.GetMonstersListConfig();
            IReadOnlyList<MonsterReference> allReferences = monstersListConfig.GetAllReferences();

            foreach (MonsterReference monsterReference in allReferences)
            {
                MonsterType monsterType = monsterReference.MonsterType;
                bool containsKey = _monstersPrefabs.TryAdd(monsterType, monsterReference.MonsterPrefab);

                if (containsKey)
                    continue;

                Log.PrintError(log: $"Dictionary <rb>already contains</rb> Monster <gb>{monsterType}</gb>!");
            }
        }
        
        private bool TryGetPrefabNetworkObject(MonsterType monsterType, out NetworkObject networkObject)
        {
            networkObject = null;
            
            bool isPrefabFound = _monstersPrefabs.TryGetValue(monsterType, out MonsterEntityBase monsterPrefab);
            
            if (!isPrefabFound)
                return false;

            bool isNetworkObjectFound = monsterPrefab.TryGetComponent(out networkObject);

            if (isNetworkObjectFound)
                return true;

            Log.PrintError(log: $"Network Object of Monster <gb>({monsterType})</gb> <rb>not found</rb>!");
            return false;
        }
    }
}