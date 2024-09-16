using GameCore.Enums.Gameplay;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public interface IMonstersAssetsStorage : IWarmUp
    {
        bool TryGetAssetReference(MonsterType monsterType, out AssetReferenceGameObject assetReference);
    }
}