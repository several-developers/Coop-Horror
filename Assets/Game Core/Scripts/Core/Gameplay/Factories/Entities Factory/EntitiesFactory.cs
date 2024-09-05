using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Network;
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
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        ) : base(assetsProvider)
        {
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;
            _entitiesListConfig = assetsProvider.GetEntitiesListConfig();
            _networkManager = NetworkManager.Singleton;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;
        private readonly EntitiesListConfigMeta _entitiesListConfig;
        private readonly NetworkManager _networkManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask WarmUp() =>
            await SetupReferencesDictionary();

        public async UniTask CreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity =>
            await LoadAndCreateEntity(spawnParams);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupReferencesDictionary()
        {
            IEnumerable<AssetReferenceGameObject> allEntitiesReferences =
                _entitiesListConfig.GetAllReferences();

            await SetupReferencesDictionary<IEntity>(allEntitiesReferences);
        }

        private async UniTask LoadAndCreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams)
            where TEntity : Entity
        {
            if (!TryGetAssetGUID<TEntity>(out string guid))
                return;

            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TEntity entityPrefab;

            if (containsAssetReference)
                entityPrefab = await LoadAsset<TEntity>(assetReference);
            else
                entityPrefab = await LoadAsset<TEntity>();

            CreateEntity(entityPrefab, spawnParams, guid);
        }

        private async void CreateEntity<TEntity>(TEntity entityPrefab, EntitySpawnParams<TEntity> spawnParams,
            string guid) where TEntity : Entity
        {
            NetworkObject prefabNetworkObject;

            if (!TryGetNetworkObject())
                return;

            _dynamicPrefabsLoaderDecorator.LoadAndGetPrefab(guid, PrefabLoaded);
            
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

            void PrefabLoaded(GameObject prefab)
            {
                if (!prefab.TryGetComponent(out NetworkObject networkPrefab))
                {
                    Log.PrintError(log: $"<gb>Network Object</gb> <rb>not found</rb> for the asset '<gb>{guid}</gb>'");
                    return;
                }
                
                Vector3 worldPosition = spawnParams.WorldPosition;
                Quaternion rotation = spawnParams.Rotation;
                ulong ownerID = spawnParams.OwnerID;

                NetworkObject networkObject = _networkManager
                    .SpawnManager.InstantiateAndSpawn(
                        networkPrefab: networkPrefab,
                        ownerClientId: ownerID,
                        destroyWithScene: true,
                        position: worldPosition,
                        rotation: rotation
                    );
                
                var instance = networkObject.GetComponent<TEntity>();

                spawnParams.SendSuccessCallback(instance);
            }
            
            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);
        }
    }
}