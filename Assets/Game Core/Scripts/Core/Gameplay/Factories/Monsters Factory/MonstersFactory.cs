using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Monsters
{
    public class MonstersFactory : IMonstersFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersFactory(IEntitiesFactory entitiesFactory, IConfigsProvider configsProvider)
        {
            _entitiesFactory = entitiesFactory;
            _monstersListConfig = configsProvider.GetConfig<MonstersListConfigMeta>();
            _prefabsKeysDictionary = new Dictionary<MonsterType, AssetReferenceGameObject>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IEntitiesFactory _entitiesFactory;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly Dictionary<MonsterType, AssetReferenceGameObject> _prefabsKeysDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask WarmUp() =>
            await SetupReferencesDictionary();
        
        public void CreateMonster<TMonsterEntity>(MonsterType monsterType,
            EntitySpawnParams<TMonsterEntity> spawnParams) where TMonsterEntity : MonsterEntityBase
        {
            if (!TryGetAssetReference(monsterType, out AssetReferenceGameObject assetReference))
            {
                spawnParams.FailCallbackEvent += _ => AssetReferenceNotFoundError(monsterType);
                return;
            }
            
            spawnParams.SetAssetReference(assetReference);
            _entitiesFactory.CreateEntityDynamic(spawnParams);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void AssetReferenceNotFoundError(MonsterType monsterType) =>
            Debug.LogError($"Asset Reference not found for '{monsterType.GetNiceName()}'!");

        private async UniTask SetupReferencesDictionary()
        {
            IEnumerable<MonsterReference> allReferences = _monstersListConfig.GetAllReferences();

            foreach (MonsterReference monsterReference in allReferences)
            {
                MonsterType monsterType = monsterReference.MonsterType;
                bool containsKey = _prefabsKeysDictionary.ContainsKey(monsterType);

                if (containsKey)
                {
                    Log.PrintError(log: $"Dictionary <rb>already contains</rb> Monster <gb>{monsterType}</gb>!");
                    continue;
                }

                AssetReferenceGameObject assetReference = monsterReference.AssetReference;

                var entity = await _entitiesFactory.LoadAndReleaseAsset<IEntity>(assetReference);
                Type entityType = entity.GetType();
                
                _entitiesFactory.AddDynamicAsset(entityType, assetReference);
                _prefabsKeysDictionary.Add(monsterType, assetReference);
            }
        }

        private bool TryGetAssetReference(MonsterType monsterType, out AssetReferenceGameObject assetReference) =>
            _prefabsKeysDictionary.TryGetValue(monsterType, out assetReference);
    }
}