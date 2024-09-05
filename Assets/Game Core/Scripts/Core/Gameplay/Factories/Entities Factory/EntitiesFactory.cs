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

        public void CreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity
        {
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
                
                NetworkSpawnManager spawnManager = NetworkManager.Singleton.SpawnManager;

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
    }
}