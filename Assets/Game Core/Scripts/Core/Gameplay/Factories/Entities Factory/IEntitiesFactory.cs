﻿using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Utilities;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Entities
{
    public interface IEntitiesFactory
    {
        UniTask WarmUp();
        UniTask LoadAssetReference<T>(AssetReference assetReference) where T : class;
        void CreateEntity<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity;
        void CreateEntityOld<TEntity>(EntitySpawnParams<TEntity> spawnParams) where TEntity : Entity;
    }
}