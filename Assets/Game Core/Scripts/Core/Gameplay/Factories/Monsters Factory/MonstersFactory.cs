using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Monsters
{
    public class MonstersFactory : IMonstersFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersFactory(IEntitiesFactory entitiesFactory, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _entitiesFactory = entitiesFactory;
            _monstersListConfig = gameplayConfigsProvider.GetMonstersListConfig();
            _prefabsKeysDictionary = new Dictionary<MonsterType, AssetReferenceGameObject>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IEntitiesFactory _entitiesFactory;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly Dictionary<MonsterType, AssetReferenceGameObject> _prefabsKeysDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask WarmUp() =>
            await SetupReferencesDictionary();

        public async UniTask SpawnMonster<TMonsterEntity>(MonsterType monsterType, Vector3 worldPosition,
            Quaternion rotation, Action<string> fail = null, Action<TMonsterEntity> success = null)
            where TMonsterEntity : MonsterEntityBase
        {
            var isAssetReferenceFound =
                _prefabsKeysDictionary.TryGetValue(monsterType, out AssetReferenceGameObject assetReference);

            if (!isAssetReferenceFound)
            {
                fail?.Invoke(obj: $"Asset Reference not found for '{monsterType.GetNiceName()}'!");
                return;
            }

            await _entitiesFactory.CreateEntity(assetReference, worldPosition, rotation, fail, success);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

                await _entitiesFactory.LoadAssetReference<IEntity>(assetReference);
                _prefabsKeysDictionary.Add(monsterType, assetReference);
            }
        }
    }
}