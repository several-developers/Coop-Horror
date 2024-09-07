using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Entities
{
    public class EntitiesFactory : AddressablesFactoryBase<Type>, IEntitiesFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesFactory(
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        ) : base(assetsProvider)
        {
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;
            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;
        private readonly EntitiesListConfigMeta _entitiesListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public async UniTask CreateEntity<TEntity>(SpawnParams<TEntity> spawnParams) where TEntity : Entity =>
            await LoadAndCreateEntity(spawnParams);

        public void CreateEntityDynamic<TEntity>(SpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;
            string guid;

            if (containsAssetReference)
            {
                guid = assetReference.AssetGUID;
            }
            else
            {
                Type entityType = typeof(TEntity);

                if (!TryGetDynamicAssetGUID(entityType, out guid))
                {
                    spawnParams.SendFailCallback(reason: $"Asset GUID for '{typeof(TEntity)}' not found!");
                    return;
                }
            }

            _dynamicPrefabsLoaderDecorator.LoadAndGetPrefab(
                guid: guid,
                loadCallback: prefabNetworkObject => EntityPrefabLoaded(prefabNetworkObject, spawnParams)
            );
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<AssetReferenceGameObject> allReferences = _entitiesListConfig.GetAllReferences();
            IEnumerable<AssetReferenceGameObject> allGlobalReferences = _entitiesListConfig.GetAllGlobalReferences();
            IEnumerable<AssetReferenceGameObject> allDynamicReferences = _entitiesListConfig.GetAllDynamicReferences();

            foreach (AssetReferenceGameObject assetReference in allReferences)
            {
                Type entityType = await GetAssetTypeAfterLoadAndRelease(assetReference);
                AddAsset(entityType, assetReference);
            }

            foreach (AssetReferenceGameObject assetReference in allGlobalReferences)
            {
                Type entityType = await GetAssetTypeAfterLoadAndRelease(assetReference);
                AddAsset(entityType, assetReference);
            }

            foreach (AssetReferenceGameObject assetReference in allDynamicReferences)
            {
                Type entityType = await GetAssetTypeAfterLoadAndRelease(assetReference);
                AddDynamicAsset(entityType, assetReference);
            }

            // LOCAL METHODS: -----------------------------

            async UniTask<Type> GetAssetTypeAfterLoadAndRelease(AssetReference assetReference)
            {
                var entity = await LoadAndReleaseAsset<Entity>(assetReference);
                Type entityType = entity.GetType();
                return entityType;
            }
        }

        private static void EntityPrefabLoaded<TEntity>(GameObject prefab, SpawnParams<TEntity> spawnParams)
            where TEntity : Entity
        {
            if (prefab == null)
            {
                SendFailCallback(reason: "Prefab not found!");
                return;
            }

            if (!prefab.TryGetComponent(out NetworkObject prefabNetworkObject))
                return;

            NetworkObject instanceNetworkObject = InstantiateEntity();
            var entityInstance = instanceNetworkObject.GetComponent<TEntity>();

            spawnParams.SendSuccessCallback(entityInstance);

            // LOCAL METHODS: -----------------------------

            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);

            NetworkObject InstantiateEntity()
            {
                Vector3 worldPosition = spawnParams.WorldPosition;
                Quaternion rotation = spawnParams.Rotation;
                ulong ownerID = spawnParams.OwnerID;

                NetworkSpawnManager spawnManager = GetNetworkSpawnManager();

                NetworkObject networkObject = spawnManager.InstantiateAndSpawn(
                    networkPrefab: prefabNetworkObject,
                    ownerClientId: ownerID,
                    destroyWithScene: true,
                    position: worldPosition,
                    rotation: rotation
                );

                return networkObject;
            }
        }

        private async UniTask LoadAndCreateEntity<TEntity>(SpawnParams<TEntity> spawnParams)
            where TEntity : Entity
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TEntity prefab;

            if (containsAssetReference)
            {
                prefab = await LoadAsset<TEntity>(assetReference);
            }
            else
            {
                Type key = typeof(TEntity);
                prefab = await LoadAsset<TEntity>(key);
            }

            CreateEntity(prefab, spawnParams);
        }

        private static void CreateEntity<TEntity>(TEntity entityPrefab, SpawnParams<TEntity> spawnParams)
            where TEntity : Entity
        {
            NetworkObject prefabNetworkObject = null;

            if (!TryGetNetworkObject())
                return;

            NetworkObject instanceNetworkObject = InstantiateNetworkObject();
            var instance = instanceNetworkObject.GetComponent<TEntity>();

            spawnParams.SendSuccessCallback(instance);

            // LOCAL METHODS: -----------------------------

            bool TryGetNetworkObject()
            {
                bool isPrefabFound = entityPrefab != null;

                if (!isPrefabFound)
                {
                    SendFailCallback(reason: "Entity prefab not found!");
                    return false;
                }

                bool isNetworkObjectFound = entityPrefab.TryGetComponent(out prefabNetworkObject);

                if (isNetworkObjectFound)
                    return true;

                SendFailCallback(reason: "Network Object not found!");
                return false;
            }

            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);

            NetworkObject InstantiateNetworkObject()
            {
                Vector3 worldPosition = spawnParams.WorldPosition;
                Quaternion rotation = spawnParams.Rotation;
                ulong ownerID = spawnParams.OwnerID;

                NetworkSpawnManager spawnManager = GetNetworkSpawnManager();

                NetworkObject networkObject = spawnManager.InstantiateAndSpawn(
                    networkPrefab: prefabNetworkObject,
                    ownerClientId: ownerID,
                    destroyWithScene: true,
                    position: worldPosition,
                    rotation: rotation
                );

                return networkObject;
            }
        }

        private static NetworkSpawnManager GetNetworkSpawnManager() =>
            NetworkManager.Singleton.SpawnManager;
    }
}