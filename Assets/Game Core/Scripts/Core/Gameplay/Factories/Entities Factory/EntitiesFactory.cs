using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.EntitiesPrefabs;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Entities
{
    public class EntitiesFactory : AddressablesFactoryBase, IEntitiesFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesFactory(IAssetsProvider assetsProvider, IEntitiesPrefabsProvider entitiesPrefabsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider) : base(assetsProvider)
        {
            _entitiesPrefabsProvider = entitiesPrefabsProvider;
            _entitiesListConfig = gameplayConfigsProvider.GetEntitiesListConfig();
            _networkManager = NetworkManager.Singleton;
            _serverID = NetworkHorror.ServerID;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IEntitiesPrefabsProvider _entitiesPrefabsProvider;
        private readonly EntitiesListConfigMeta _entitiesListConfig;
        private readonly NetworkManager _networkManager;
        private readonly ulong _serverID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask WarmUp() =>
            await SetupReferencesDictionary();

        public async UniTask CreateEntity<TEntity>(Vector3 worldPosition, Action<string> fail = null,
            Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateFinalEntity(worldPosition, Quaternion.identity, _serverID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(Vector3 worldPosition, ulong ownerID, Action<string> fail = null,
            Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateFinalEntity(worldPosition, Quaternion.identity, ownerID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(Vector3 worldPosition, Quaternion rotation,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateFinalEntity(worldPosition, rotation, _serverID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(Vector3 worldPosition, Quaternion rotation, ulong ownerID,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateFinalEntity(worldPosition, Quaternion.identity, ownerID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateEntity(assetReference, worldPosition, Quaternion.identity, _serverID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition, ulong ownerID,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateEntity(assetReference, worldPosition, Quaternion.identity, ownerID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition,
            Quaternion rotation, Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity
        {
            await CreateEntity(assetReference, worldPosition, rotation, _serverID, fail, success);
        }

        public async UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition,
            Quaternion rotation, ulong ownerID, Action<string> fail = null, Action<TEntity> success = null)
            where TEntity : Entity
        {
            await CreateFinalEntity<TEntity>(assetReference, worldPosition, rotation, ownerID);
        }

        public async UniTask CreateEntity<TEntity>(EntitySpawnParams spawnParams) where TEntity : Entity
        {
            await CreateFinalEntity<TEntity>(spawnParams);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupReferencesDictionary()
        {
            IEnumerable<AssetReferenceGameObject> allEntitiesReferences =
                _entitiesListConfig.GetAllEntitiesReferences();

            await SetupReferencesDictionary<IEntity>(allEntitiesReferences);
        }

        private async UniTask CreateFinalEntity<TEntity>(Vector3 worldPosition, Quaternion rotation, ulong ownerID,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity
        {
            var entityPrefab = await LoadAsset<TEntity>();
            CreateFinalEntity(entityPrefab, worldPosition, rotation, ownerID, fail, success);
        }

        private async UniTask CreateFinalEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition,
            Quaternion rotation, ulong ownerID, Action<string> fail = null, Action<TEntity> success = null)
            where TEntity : Entity
        {
            var entityPrefab = await LoadAsset<TEntity>(assetReference);
            CreateFinalEntity(entityPrefab, worldPosition, rotation, ownerID, fail, success);
        }

        private async UniTask CreateFinalEntity<TEntity>(EntitySpawnParams spawnParams) where TEntity : Entity
        {
            Vector3 worldPosition = spawnParams.WorldPosition;
            var entityPrefab = await LoadAsset<TEntity>(assetReference);
            CreateFinalEntity(entityPrefab, worldPosition, rotation, ownerID, fail, success);
        }

        private void CreateFinalEntity<TEntity>(
            TEntity entityPrefab,
            Vector3 worldPosition,
            Quaternion rotation,
            ulong ownerID,
            Action<string> fail = null,
            Action<TEntity> success = null
        ) where TEntity : Entity
        {
            bool isPrefabFound = entityPrefab == null;

            if (!isPrefabFound)
            {
                fail?.Invoke(obj: "Entity prefab not found!");
                return;
            }

            bool isNetworkObjectFound = entityPrefab.TryGetComponent(out NetworkObject prefabNetworkObject);

            if (!isNetworkObjectFound)
            {
                fail?.Invoke(obj: "Network Object not found!");
                return;
            }

            NetworkObject networkObject = _networkManager.SpawnManager
                .InstantiateAndSpawn(prefabNetworkObject, ownerID, destroyWithScene: true, position: worldPosition);

            var instance = networkObject.GetComponent<TEntity>();
            success?.Invoke(instance);
        }

        private bool TryGetEntityPrefab<TEntityType>(out Entity entityPrefab) where TEntityType : IEntity =>
            _entitiesPrefabsProvider.TryGetEntityPrefab<TEntityType>(out entityPrefab);
    }
}