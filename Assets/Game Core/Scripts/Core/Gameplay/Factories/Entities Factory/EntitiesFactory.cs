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

        public void CreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            Debug.Log("Trying create player");
            if (!TryGetAssetGUID<TEntity>(out string guid))
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
            IEnumerable<AssetReferenceGameObject> allEntitiesReferences = _entitiesListConfig.GetAllReferences();

            await SetupReferencesDictionary<IEntity>(allEntitiesReferences);
        }

        private void EntityPrefabLoaded<TEntity>(NetworkObject prefabNetworkObject,
            EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
            if (prefabNetworkObject == null)
            {
                SendFailCallback(reason: "Network Object not found!");
                return;
            }

            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;
            ulong ownerID = spawnParams.OwnerID;

            Debug.LogWarning("SPAWNING");

            _networkManager.SpawnManager.InstantiateAndSpawn(
                networkPrefab: prefabNetworkObject,
                ownerClientId: ownerID,
                destroyWithScene: true,
                position: worldPosition,
                rotation: rotation
            );

            return;
            NetworkObject instanceNetworkObject = InstantiateEntity();
            var instance = instanceNetworkObject.GetComponent<TEntity>();

            spawnParams.SendSuccessCallback(instance);

            // LOCAL METHODS: -----------------------------

            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);

            NetworkObject InstantiateEntity()
            {
                Vector3 worldPosition = spawnParams.WorldPosition;
                Quaternion rotation = spawnParams.Rotation;
                ulong ownerID = spawnParams.OwnerID;

                NetworkObject networkObject = _networkManager
                    .SpawnManager.InstantiateAndSpawn(
                        networkPrefab: prefabNetworkObject,
                        ownerClientId: ownerID,
                        destroyWithScene: true,
                        position: worldPosition,
                        rotation: rotation
                    );

                return networkObject;
            }
        }
    }
}