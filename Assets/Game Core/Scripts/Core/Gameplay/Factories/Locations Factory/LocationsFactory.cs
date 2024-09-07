using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.LocationsList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Locations
{
    public class LocationsFactory : AddressablesFactoryBase<LocationName>, ILocationsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsFactory(
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        ) : base(assetsProvider)
        {
            _locationsListConfig = configsProvider.GetConfig<LocationsListConfigMeta>();
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LocationsListConfigMeta _locationsListConfig;
        private readonly IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public async UniTask CreateLocation<TLocation>(LocationName locationName, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
        {
            await LoadAndCreateLocation(locationName, spawnParams);
        }

        public void CreateLocationDynamic<TLocation>(LocationName locationName, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
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
                if (!TryGetDynamicAssetGUID(locationName, out guid))
                {
                    spawnParams.SendFailCallback(reason: $"Asset GUID for '{typeof(TLocation)}' not found!");
                    return;
                }
            }

            _dynamicPrefabsLoaderDecorator.LoadAndGetPrefab(
                guid: guid,
                loadCallback: prefabNetworkObject => LocationPrefabLoaded(prefabNetworkObject, spawnParams)
            );
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<LocationsListConfigMeta.LocationReference> allLocationsReferences =
                _locationsListConfig.GetAllLocationsReferences();

            foreach (LocationsListConfigMeta.LocationReference locationReference in allLocationsReferences)
            {
                AssetReferenceGameObject locationPrefabAsset = locationReference.LocationPrefabAsset;

                await LoadAndReleaseAsset<LocationManager>(locationPrefabAsset);

                LocationName locationName = locationReference.LocationMeta.LocationName;
                AddAsset(locationName, locationPrefabAsset);
            }
        }

        private static void LocationPrefabLoaded<TLocation>(GameObject prefab, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
        {
            if (prefab == null)
            {
                SendFailCallback(reason: "Prefab not found!");
                return;
            }

            if (!prefab.TryGetComponent(out NetworkObject prefabNetworkObject))
                return;

            NetworkObject instanceNetworkObject = InstantiateEntity();
            var entityInstance = instanceNetworkObject.GetComponent<TLocation>();

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

        private async UniTask LoadAndCreateLocation<TLocation>(LocationName locationName,
            SpawnParams<TLocation> spawnParams) where TLocation : LocationManager
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TLocation prefab;

            if (containsAssetReference)
                prefab = await LoadAsset<TLocation>(assetReference);
            else
                prefab = await LoadAsset<TLocation>(locationName);

            CreateLocationDefault(prefab, spawnParams);
        }

        private static void CreateLocationDefault<TLocation>(TLocation entityPrefab, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
        {
            if (!TryGetNetworkObject())
                return;

            TLocation locationInstance = InstantiateLocation();
            spawnParams.SendSuccessCallback(locationInstance);

            // LOCAL METHODS: -----------------------------

            bool TryGetNetworkObject()
            {
                bool isPrefabFound = entityPrefab != null;

                if (!isPrefabFound)
                {
                    SendFailCallback(reason: "Entity prefab not found!");
                    return false;
                }
                
                return false;
            }

            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);

            TLocation InstantiateLocation()
            {
                Vector3 worldPosition = spawnParams.WorldPosition;
                Quaternion rotation = spawnParams.Rotation;

                return Object.Instantiate(entityPrefab, worldPosition, rotation);
            }
        }
        
        private static void CreateLocationNetwork<TLocation>(TLocation entityPrefab, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
        {
            NetworkObject prefabNetworkObject = null;

            if (!TryGetNetworkObject())
                return;

            NetworkObject instanceNetworkObject = InstantiateNetworkObject();
            var instance = instanceNetworkObject.GetComponent<TLocation>();

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