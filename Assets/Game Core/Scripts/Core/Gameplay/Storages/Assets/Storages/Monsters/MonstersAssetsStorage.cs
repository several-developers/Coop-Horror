using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Configs.Global.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public class MonstersAssetsStorage : IMonstersAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersAssetsStorage(IEntitiesAssetsStorage entitiesAssetsStorage, IConfigsProvider configsProvider)
        {
            _entitiesAssetsStorage = entitiesAssetsStorage;
            _monstersListConfig = configsProvider.GetConfig<MonstersListConfigMeta>();
            _prefabsKeysDictionary = new Dictionary<MonsterType, AssetReferenceGameObject>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IEntitiesAssetsStorage _entitiesAssetsStorage;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly Dictionary<MonsterType, AssetReferenceGameObject> _prefabsKeysDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public bool TryGetAssetReference(MonsterType monsterType, out AssetReferenceGameObject assetReference)
        {
            bool isMonsterAssetFound = _prefabsKeysDictionary.TryGetValue(monsterType, out assetReference);

            if (!isMonsterAssetFound)
                Debug.LogError(message: $"Asset Reference not found for '{monsterType.GetNiceName()}'!");

            return isMonsterAssetFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<MonstersListConfigMeta.MonsterReference> allMonstersReferences =
                _monstersListConfig.GetAllMonstersReferences();

            foreach (MonstersListConfigMeta.MonsterReference monsterReference in allMonstersReferences)
            {
                MonsterType monsterType = monsterReference.MonsterType;
                bool containsKey = _prefabsKeysDictionary.ContainsKey(monsterType);

                if (containsKey)
                {
                    Log.PrintError(log: $"Dictionary <rb>already contains</rb> Monster <gb>{monsterType}</gb>!");
                    continue;
                }

                AssetReferenceGameObject monsterPrefabAsset = monsterReference.AssetReference;

                var entity = await _entitiesAssetsStorage.LoadAndReleaseAsset<IEntity>(monsterPrefabAsset);
                Type entityType = entity.GetType();

                _entitiesAssetsStorage.AddDynamicAsset(entityType, monsterPrefabAsset);
                _prefabsKeysDictionary.Add(monsterType, monsterPrefabAsset);
            }
        }
    }
}