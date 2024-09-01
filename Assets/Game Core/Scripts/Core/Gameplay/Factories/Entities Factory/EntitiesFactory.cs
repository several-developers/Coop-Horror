using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories.Entities
{
    public class EntitiesFactory : IEntitiesFactory, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesFactory(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _networkManager = NetworkManager.Singleton;
            _entitiesListConfig = gameplayConfigsProvider.GetEntitiesListConfig();
            _prefabsDictionary = new Dictionary<Type, Entity>();
            _serverID = NetworkHorror.ServerID;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkManager _networkManager;
        private readonly EntitiesListConfigMeta _entitiesListConfig;
        private readonly Dictionary<Type, Entity> _prefabsDictionary;
        private readonly ulong _serverID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize() => SetupPrefabsDictionary();

        public bool TryCreateEntity<TEntityType>(Vector3 worldPosition, out Entity entity)
            where TEntityType : IEntity
        {
            return TryCreateEntity<TEntityType>(worldPosition, rotation: Quaternion.identity, _serverID, out entity);
        }

        public bool TryCreateEntity<TEntityType>(Vector3 worldPosition, ulong ownerID, out Entity entity)
            where TEntityType : IEntity
        {
            return TryCreateEntity<TEntityType>(worldPosition, rotation: Quaternion.identity, ownerID, out entity);
        }

        public bool TryCreateEntity<TEntityType>(Vector3 worldPosition, Quaternion rotation, out Entity entity)
            where TEntityType : IEntity
        {
            return TryCreateEntity<TEntityType>(worldPosition, rotation, _serverID, out entity);
        }

        public bool TryCreateEntity<TEntityType>(Vector3 worldPosition, Quaternion rotation, ulong ownerID,
            out Entity entity) where TEntityType : IEntity
        {
            entity = null;

            bool isPrefabFound = TryGetEntityPrefab<TEntityType>(out Entity entityPrefab);

            if (!isPrefabFound)
                return false;

            bool isNetworkObjectFound = entityPrefab.TryGetComponent(out NetworkObject prefabNetworkObject);

            if (!isNetworkObjectFound)
                return false;
            
            NetworkObject networkObject = _networkManager.SpawnManager
                .InstantiateAndSpawn(prefabNetworkObject, ownerID, destroyWithScene: true, position: worldPosition);

            entity = networkObject.GetComponent<Entity>();
            return true;
        }

        public bool TryGetEntityPrefab<TEntityType>(out Entity entityPrefab) where TEntityType : IEntity
        {
            Type type = typeof(TEntityType);
            bool isPrefabFound = _prefabsDictionary.TryGetValue(type, out entityPrefab);

            if (!isPrefabFound)
                Log.PrintError(log: $"<gb>Entity '{type.Name}' prefab</gb> was <rb>not found</rb>!");

            return isPrefabFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPrefabsDictionary()
        {
            IEnumerable<Entity> allEntities = _entitiesListConfig.GetAllEntities();

            foreach (Entity entity in allEntities)
            {
                Type type = entity.GetType();
                _prefabsDictionary.Add(type, entity);
            }
        }
    }
}