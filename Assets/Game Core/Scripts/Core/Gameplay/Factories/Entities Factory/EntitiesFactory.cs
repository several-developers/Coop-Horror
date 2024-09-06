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
    public class EntitiesFactory : AddressablesFactoryBase, IEntitiesFactory
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

        public async UniTask WarmUp() =>
            await SetupReferencesDictionary();

        public async UniTask CreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity =>
            await LoadAndCreateEntity(spawnParams);

        public void DynamicCreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            if (!TryGetDynamicAssetGUID<TEntity>(out string guid))
            {
                spawnParams.SendFailCallback(reason: $"Asset GUID for '{typeof(TEntity)}' not found!");
                return;
            }

            _dynamicPrefabsLoaderDecorator.LoadAndGetPrefab(
                guid: guid,
                loadCallback: prefabNetworkObject => EntityPrefabLoaded(prefabNetworkObject, spawnParams)
            );
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupReferencesDictionary()
        {
            IEnumerable<AssetReferenceGameObject> allReferences = _entitiesListConfig.GetAllReferences();
            IEnumerable<AssetReferenceGameObject> allDynamicReferences = _entitiesListConfig.GetAllDynamicReferences();

            await SetupReferencesDictionary<Entity>(allReferences);
            await SetupDynamicReferencesDictionary<Entity>(allDynamicReferences);
        }

        private void EntityPrefabLoaded<TEntity>(NetworkObject prefabNetworkObject,
            EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            if (prefabNetworkObject == null)
            {
                SendFailCallback(reason: "Network Object not found!");
                return;
            }

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

        private async UniTask LoadAndCreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams)
            where TEntity : Entity
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TEntity entityPrefab;

            if (containsAssetReference)
                entityPrefab = await LoadAsset<TEntity>(assetReference);
            else
                entityPrefab = await LoadAsset<TEntity>();

            CreateEntity(entityPrefab, spawnParams);
        }

        private void CreateEntity<TEntity>(TEntity entityPrefab, EntitySpawnParams<TEntity> spawnParams)
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