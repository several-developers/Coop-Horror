using GameCore.Gameplay.Entities;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Providers.Gameplay.EntitiesPrefabs
{
    public interface IEntitiesPrefabsProvider
    {
        bool TryGetEntityPrefab<TEntityType>(out Entity entityPrefab) where TEntityType : IEntity;
        bool TryGetEntityAsset<TEntityType>(out AssetReferenceGameObject assetReference) where TEntityType : IEntity;
    }
}