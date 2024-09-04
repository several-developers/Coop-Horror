using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Entities
{
    public interface IEntitiesFactory
    {
        UniTask WarmUp();
        UniTask LoadAssetReference<T>(AssetReference assetReference) where T : class;

        UniTask CreateEntity<TEntity>(Vector3 worldPosition, Action<string> fail = null,
            Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(Vector3 worldPosition, ulong ownerID, Action<string> fail = null,
            Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(Vector3 worldPosition, Quaternion rotation, Action<string> fail = null,
            Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(Vector3 worldPosition, Quaternion rotation, ulong ownerID,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition, Action<string> fail = null,
            Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition, ulong ownerID,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity;
        
        UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition, Quaternion rotation,
            Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(AssetReference assetReference, Vector3 worldPosition, Quaternion rotation,
            ulong ownerID, Action<string> fail = null, Action<TEntity> success = null) where TEntity : Entity;

        UniTask CreateEntity<TEntity>(EntitySpawnParams spawnParams) where TEntity : Entity;
    }
}