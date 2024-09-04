using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Utilities;
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

        public EntitiesFactory(IAssetsProvider assetsProvider, IGameplayConfigsProvider gameplayConfigsProvider)
            : base(assetsProvider)
        {
            _entitiesListConfig = gameplayConfigsProvider.GetEntitiesListConfig();
            _networkManager = NetworkManager.Singleton;
        }

        // FIELDS: --------------------------------------------------------------------------------

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

            NetworkObject networkObject = InstantiateNetworkObject();
            var instance = networkObject.GetComponent<TEntity>();

            spawnParams.SendSuccessCallback(instance);

            // LOCAL METHODS: -----------------------------

            bool TryGetNetworkObject()
            {
                bool isPrefabFound = entityPrefab == null;

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

                return _networkManager.SpawnManager.InstantiateAndSpawn(prefabNetworkObject, ownerID,
                    destroyWithScene: true, position: worldPosition);
            }
        }
    }
}