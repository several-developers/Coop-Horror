using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.EntitiesPrefabs;
using GameCore.Infrastructure.Providers.Global;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Entities
{
    public class EntitiesFactory : IEntitiesFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesFactory(IAssetsProvider assetsProvider, IEntitiesPrefabsProvider entitiesPrefabsProvider)
        {
            _assetsProvider = assetsProvider;
            _entitiesPrefabsProvider = entitiesPrefabsProvider;
            _networkManager = NetworkManager.Singleton;
            _serverID = NetworkHorror.ServerID;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IAssetsProvider _assetsProvider;
        private readonly IEntitiesPrefabsProvider _entitiesPrefabsProvider;
        private readonly NetworkManager _networkManager;
        private readonly ulong _serverID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask TryCreateEntity<TEntityType>(Vector3 worldPosition, ulong ownerID,
            Action<string> fail = null, Action<TEntityType> success = null) where TEntityType : IEntity
        {
            bool isAssetReferenceFound =
                _entitiesPrefabsProvider.TryGetEntityAsset<TEntityType>(out AssetReferenceGameObject assetReference);

            if (!isAssetReferenceFound)
            {
                fail?.Invoke(obj: "Asset Reference not found.");
                return;
            }

            GameObject instance =
                await Addressables.InstantiateAsync(assetReference, worldPosition, Quaternion.identity).Task;

            bool isEntityComponentFound = instance.TryGetComponent(out TEntityType entity);

            if (isEntityComponentFound)
                success?.Invoke(entity);
            else
                fail?.Invoke(obj: "Entity component not found.");
        }

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool TryGetEntityPrefab<TEntityType>(out Entity entityPrefab) where TEntityType : IEntity =>
            _entitiesPrefabsProvider.TryGetEntityPrefab<TEntityType>(out entityPrefab);
    }
}