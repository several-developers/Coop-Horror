using GameCore.Enums.Gameplay;
using GameCore.Gameplay.AssetsStorages;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Utilities;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Monsters
{
    public class MonstersFactory : IMonstersFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersFactory(IEntitiesFactory entitiesFactory, IMonstersAssetsStorage monstersAssetsStorage)
        {
            _entitiesFactory = entitiesFactory;
            _monstersAssetsStorage = monstersAssetsStorage;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IEntitiesFactory _entitiesFactory;
        private readonly IMonstersAssetsStorage _monstersAssetsStorage;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CreateMonsterDynamic<TMonsterEntity>(MonsterType monsterType,
            SpawnParams<TMonsterEntity> spawnParams) where TMonsterEntity : MonsterEntityBase
        {
            bool isMonsterAssetFound =
                _monstersAssetsStorage.TryGetAssetReference(monsterType, out AssetReferenceGameObject assetReference);

            if (!isMonsterAssetFound)
                return;

            spawnParams.SetAssetReference(assetReference);
            _entitiesFactory.CreateEntityDynamic(spawnParams);
        }
    }
}