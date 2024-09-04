using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.EntitiesList;
using GameCore.Gameplay.Entities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Infrastructure.Providers.Gameplay.EntitiesPrefabs
{
    public class EntitiesPrefabsProvider : AssetsProviderBase, IEntitiesPrefabsProvider, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesPrefabsProvider(
            IAssetsProvider assetsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _assetsProvider = assetsProvider;
            _entitiesListConfig = gameplayConfigsProvider.GetEntitiesListConfig();
            _prefabsDictionary = new Dictionary<Type, Entity>();
            _referencesDictionary = new Dictionary<Type, AssetReferenceGameObject>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IAssetsProvider _assetsProvider;
        private readonly EntitiesListConfigMeta _entitiesListConfig;
        private readonly Dictionary<Type, Entity> _prefabsDictionary;
        private readonly Dictionary<Type, AssetReferenceGameObject> _referencesDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async void Initialize()
        {
            SetupReferencesDictionary();
            await PreloadPrefabs();
        }

        public bool TryGetEntityPrefab<TEntityType>(out Entity entityPrefab) where TEntityType : IEntity
        {
            Type type = typeof(TEntityType);
            bool isPrefabFound = _prefabsDictionary.TryGetValue(type, out entityPrefab);

            if (!isPrefabFound)
                Log.PrintError(log: $"<gb>Entity '{type.Name}' prefab</gb> was <rb>not found</rb>!");

            return isPrefabFound;
        }

        public bool TryGetEntityAsset<TEntityType>(out AssetReferenceGameObject assetReference)
            where TEntityType : IEntity
        {
            Type type = typeof(TEntityType);
            bool isAssetReferenceFound = _referencesDictionary.TryGetValue(type, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Entity '{type.Name}' prefab</gb> was <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupReferencesDictionary()
        {
            IEnumerable<AssetReferenceGameObject>
                allEntitiesReferences = _entitiesListConfig.GetAllEntitiesReferences();

            foreach (AssetReferenceGameObject assetReference in allEntitiesReferences)
            {
                if (!assetReference.editorAsset.TryGetComponent(out Entity entity))
                {
                    Log.PrintError(log: $"<gb>Entity '' asset</gb> was <rb>not found</rb>!");
                    continue;
                }

                Type type = entity.GetType();
                bool success = _referencesDictionary.TryAdd(type, assetReference);

                if (success)
                    continue;

                Log.PrintError(log: $"Key '<gb>{type.Name}</gb>' <rb>already exists</rb>!");
            }
        }

        private async UniTask PreloadPrefabs()
        {
            IEnumerable<AssetReferenceGameObject>
                allEntitiesReferences = _entitiesListConfig.GetAllEntitiesReferences();

            foreach (AssetReferenceGameObject assetReference in allEntitiesReferences)
            {
                var prefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
                AddPrefabToDictionary(prefab);
            }

            // string log = Log.HandleLog("<gb======== PREFABS LOADED ========");
            // Debug.Log(log);
        }

        private void AddPrefabToDictionary(GameObject prefab)
        {
            if (!prefab.TryGetComponent(out Entity entity))
            {
                Log.PrintError(log: $"Prefab '<gb>{prefab.name}</gb>' doesn't have <gb>Entity</gb> component!");
                return;
            }

            Type type = entity.GetType();
            bool success = _prefabsDictionary.TryAdd(type, entity);

            if (success)
                return;

            Log.PrintError(log: $"Dictionary <rb>already have</rb> '<gb>{prefab.name}</gb>' prefab!");
        }
    }
}